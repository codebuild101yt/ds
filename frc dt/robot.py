import socket
import struct
import numpy as np
import cv2
import threading
import wpilib
from wpilib import Field2d, SmartDashboard
from wpimath.geometry import Pose2d, Rotation2d
from cscore import CameraServer
import time
import math

# ===== CONFIG =====
UDP_POSE_IP = "127.0.0.1"
UDP_POSE_PORT = 5805
UNITY_TO_METERS = 1.0
VELOCITY_SCALE = 0.5

TCP_IP = "127.0.0.1"
TCP_PORT = 8080
FRAME_WIDTH = 320
FRAME_HEIGHT = 240

RECONNECT_DELAY = 1.0  # seconds before trying to reconnect
# ==================

# UDP socket for pose telemetry
pose_sock = socket.socket(socket.AF_INET, socket.SOCK_DGRAM)
pose_sock.bind((UDP_POSE_IP, UDP_POSE_PORT))
pose_sock.setblocking(False)
print(f"Listening for UDP pose on {UDP_POSE_IP}:{UDP_POSE_PORT}")


def compute_velocity(last_x, last_y, current_x, current_y, dt):
    if dt <= 0:
        return 0.0, 0.0
    return (current_x - last_x) / dt, (current_y - last_y) / dt


class MyRobot(wpilib.TimedRobot):
    def robotInit(self):
        self.field = Field2d()
        SmartDashboard.putData("Field", self.field)
        self.velocity_arrow = self.field.getObject("VelocityArrow")
        self.trajectory_points = []

        self.latest_pose = None
        self.gyro_offset_deg = 0.0
        self.last_wpilib_x = None
        self.last_wpilib_y = None
        self.last_time = None

        self.frame = None
        self.cam_output = CameraServer.putVideo("DriveCam", FRAME_WIDTH, FRAME_HEIGHT)

        # Start TCP video loop with reconnect
        threading.Thread(target=self._tcp_video_loop, daemon=True).start()
        print("Robot initialized. Waiting for Unity pose and video feed...")

    def teleopPeriodic(self):
        # --- UDP Pose ---
        try:
            while True:
                data, _ = pose_sock.recvfrom(1024)
                self.latest_pose = data.decode().strip()
        except BlockingIOError:
            pass

        if self.latest_pose:
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

            current_time = time.time()
            if self.last_wpilib_x is not None and self.last_time is not None:
                dt = current_time - self.last_time
                vel_x, vel_y = compute_velocity(self.last_wpilib_x, self.last_wpilib_y, wpilib_x, wpilib_y, dt)
            else:
                vel_x = vel_y = 0.0

            self.last_wpilib_x = wpilib_x
            self.last_wpilib_y = wpilib_y
            self.last_time = current_time

            pose = Pose2d(wpilib_x, wpilib_y, Rotation2d.fromDegrees(-gyro_deg + 270))
            self.field.setRobotPose(pose)

            speed = math.hypot(vel_x, vel_y)
            angle_rad = math.atan2(vel_y, vel_x) if speed > 0 else 0.0
            self.velocity_arrow.setPose(Pose2d(wpilib_x + vel_x * VELOCITY_SCALE,
                                               wpilib_y + vel_y * VELOCITY_SCALE,
                                               Rotation2d(angle_rad)))

            self.trajectory_points.append(Pose2d(wpilib_x, wpilib_y, Rotation2d()))
            if len(self.trajectory_points) > 50:
                self.trajectory_points.pop(0)
            self.field.getObject("Trajectory").setPoses(self.trajectory_points)

            SmartDashboard.putNumber("Field Velocity X", vel_x)
            SmartDashboard.putNumber("Field Velocity Y", vel_y)
            SmartDashboard.putNumber("Velocity Magnitude", speed)

        # Update CameraServer with latest frame
        if self.frame is not None:
            self.cam_output.putFrame(self.frame)

    def _tcp_video_loop(self):
        while True:
            try:
                sock = socket.socket(socket.AF_INET, socket.SOCK_STREAM)
                sock.bind((TCP_IP, TCP_PORT))
                sock.listen(1)
                print(f"Waiting for Unity TCP connection on {TCP_IP}:{TCP_PORT}...")
                conn, addr = sock.accept()
                print(f"Unity connected: {addr}")

                buffer = b""
                while True:
                    # Read 4-byte big-endian length
                    while len(buffer) < 4:
                        data = conn.recv(4096)
                        if not data:
                            raise ConnectionError("Unity disconnected")
                        buffer += data
                    length_bytes = buffer[:4]
                    buffer = buffer[4:]
                    frame_len = struct.unpack(">I", length_bytes)[0]

                    # Read full JPEG frame
                    while len(buffer) < frame_len:
                        data = conn.recv(4096)
                        if not data:
                            raise ConnectionError("Unity disconnected during frame")
                        buffer += data
                    frame_data = buffer[:frame_len]
                    buffer = buffer[frame_len:]

                    # Decode JPEG
                    frame = cv2.imdecode(np.frombuffer(frame_data, np.uint8), cv2.IMREAD_COLOR)
                    if frame is not None:
                        self.frame = frame
                    else:
                        print(f"Failed to decode frame of length {len(frame_data)}")
            except Exception as e:
                print(f"[TCP] Connection lost: {e}. Retrying in {RECONNECT_DELAY}s...")
                time.sleep(RECONNECT_DELAY)
