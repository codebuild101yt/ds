import socket
import wpilib
from wpilib import SmartDashboard, Field2d
from wpimath.geometry import Pose2d, Rotation2d
import time
import math
import cv2
import base64
from networktables import NetworkTables

# ===== CONFIG =====
UDP_IP = "127.0.0.1"
UDP_PORT = 5805
UNITY_TO_METERS = 1.0
VELOCITY_SCALE = 0.5
UNITY_CAMERA_URL = "http://<unity-pc-ip>:8080/video/"  # Replace with your Unity PC IP
NT4_SERVER_IP = "127.0.0.1"
CAMERA_TABLE_NAME = "CameraPublisher"
CAMERA_KEY = "NetworkTablesVideo"
FRAME_SEND_INTERVAL = 0.1  # seconds
# ==================

# UDP socket for pose data
sock = socket.socket(socket.AF_INET, socket.SOCK_DGRAM)
sock.bind((UDP_IP, UDP_PORT))
sock.setblocking(False)
print(f"Listening for UDP pose on {UDP_IP}:{UDP_PORT}")

# Initialize NetworkTables
NetworkTables.initialize(server=NT4_SERVER_IP)
camera_table = NetworkTables.getTable(CAMERA_TABLE_NAME)

class MyRobot(wpilib.TimedRobot):
    def robotInit(self):
        # Field2d
        self.field = Field2d()
        SmartDashboard.putData("Field", self.field)

        # Pose & gyro
        self.latest_pose = None
        self.gyro_offset_deg = 0.0

        # Velocity calculation
        self.last_wpilib_x = None
        self.last_wpilib_y = None
        self.last_time = None

        # Velocity arrow & trajectory
        self.velocity_arrow = self.field.getObject("VelocityArrow")
        self.trajectory_points = []

        # --- Unity camera capture ---
        self.cap = cv2.VideoCapture(UNITY_CAMERA_URL)
        if not self.cap.isOpened():
            print("WARNING: Cannot open Unity camera stream!")
        else:
            self.cap.set(cv2.CAP_PROP_FRAME_WIDTH, 320)
            self.cap.set(cv2.CAP_PROP_FRAME_HEIGHT, 240)

        self.last_frame_time = 0.0
        print("Robot init complete. Camera ready for Elastic NetworkTables widget.")

    def teleopPeriodic(self):
        # --- UDP Pose ---
        try:
            while True:
                data, _ = sock.recvfrom(1024)
                self.latest_pose = data.decode().strip()
        except BlockingIOError:
            pass

        if not self.latest_pose:
            return

        try:
            unity_x, unity_z, heading_deg = map(float, self.latest_pose.split(","))
        except ValueError:
            return

        wpilib_x = -unity_x * UNITY_TO_METERS
        wpilib_y = -unity_z * UNITY_TO_METERS

        joystick = wpilib.Joystick(0)
        for i in range(1, joystick.getButtonCount() + 1):
            if joystick.getRawButton(i):
                self.gyro_offset_deg = heading_deg
                break

        gyro_deg = (heading_deg - self.gyro_offset_deg) % 360.0
        SmartDashboard.putNumber("Gyro", gyro_deg)

        # Velocity calculation
        current_time = time.time()
        if self.last_wpilib_x is not None and self.last_time is not None:
            dt = current_time - self.last_time
            vel_x = (wpilib_x - self.last_wpilib_x) / dt if dt > 0 else 0.0
            vel_y = (wpilib_y - self.last_wpilib_y) / dt if dt > 0 else 0.0
        else:
            vel_x = 0.0
            vel_y = 0.0

        self.last_wpilib_x = wpilib_x
        self.last_wpilib_y = wpilib_y
        self.last_time = current_time

        # Update Field2d
        pose = Pose2d(wpilib_x, wpilib_y, Rotation2d.fromDegrees(-gyro_deg + 270))
        self.field.setRobotPose(pose)

        # Velocity arrow
        speed = math.hypot(vel_x, vel_y)
        angle_rad = math.atan2(vel_y, vel_x) if speed > 0 else 0.0
        arrow_tip_x = wpilib_x + vel_x * VELOCITY_SCALE
        arrow_tip_y = wpilib_y + vel_y * VELOCITY_SCALE
        self.velocity_arrow.setPose(Pose2d(arrow_tip_x, arrow_tip_y, Rotation2d(angle_rad)))

        # Trajectory line
        self.trajectory_points.append(Pose2d(wpilib_x, wpilib_y, Rotation2d()))
        if len(self.trajectory_points) > 50:
            self.trajectory_points.pop(0)
        traj_obj = self.field.getObject("Trajectory")
        traj_obj.setPoses(self.trajectory_points)

        # Dashboard numbers
        SmartDashboard.putNumber("Field Velocity X", vel_x)
        SmartDashboard.putNumber("Field Velocity Y", vel_y)
        SmartDashboard.putNumber("Velocity Magnitude", speed)

        # --- Send camera frame to Elastic via NetworkTables ---
        if self.cap.isOpened():
            now = time.time()
            if now - self.last_frame_time >= FRAME_SEND_INTERVAL:
                ret, frame = self.cap.read()
                if ret:
                    _, jpeg = cv2.imencode('.jpg', frame)
                    b64_data = base64.b64encode(jpeg).decode('utf-8')
                    html = f'<img src="data:image/jpeg;base64,{b64_data}" width="320" height="240"/>'
                    camera_table.putString(CAMERA_KEY, html)
                    self.last_frame_time = now

        # Debug
        print(f"POSE x={wpilib_x:.2f}, y={wpilib_y:.2f}, vel=({vel_x:.2f},{vel_y:.2f}), traj pts={len(self.trajectory_points)}")


if __name__ == "__main__":
    wpilib.run(MyRobot)
