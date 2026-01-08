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

# Ports must match Unity cameras
CAMERA_CONFIGS = [
    {"name": "DriveCam", "tcp_port": 8080, "width": 320, "height": 240},
    {"name": "ArmCam",   "tcp_port": 8081, "width": 320, "height": 240},
    {"name": "LiftCam",  "tcp_port": 8082, "width": 320, "height": 240},
]

RECONNECT_DELAY = 1.0  # seconds before reconnect
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


class MultiCameraServer:
    """Handles multiple TCP video feeds from Unity"""

    def __init__(self, cameras):
        self.frames = {}
        self.cams = {}
        for cam in cameras:
            self.frames[cam["name"]] = None
            self.cams[cam["name"]] = CameraServer.putVideo(cam["name"], cam["width"], cam["height"])
            threading.Thread(target=self._tcp_video_loop, args=(cam,), daemon=True).start()

    def _tcp_video_loop(self, cam):
        name = cam["name"]
        port = cam["tcp_port"]
        width = cam["width"]
        height = cam["height"]

        while True:
            try:
                sock = socket.socket(socket.AF_INET, socket.SOCK_STREAM)
                sock.bind(("0.0.0.0", port))
                sock.listen(1)
                print(f"[{name}] Waiting for TCP connection on port {port}...")
                conn, addr = sock.accept()
                print(f"[{name}] Unity connected: {addr}")

                buffer = b""
                while True:
                    # Read 4-byte big-endian length
                    while len(buffer) < 4:
                        data = conn.recv(4096)
                        if not data:
                            raise ConnectionError("Disconnected")
                        buffer += data
                    frame_len = struct.unpack(">I", buffer[:4])[0]
                    buffer = buffer[4:]

                    # Read full frame
                    while len(buffer) < frame_len:
                        data = conn.recv(4096)
                        if not data:
                            raise ConnectionError("Disconnected during frame")
                        buffer += data
                    frame_data = buffer[:frame_len]
                    buffer = buffer[frame_len:]

                    frame = cv2.imdecode(np.frombuffer(frame_data, np.uint8), cv2.IMREAD_COLOR)
                    if frame is not None:
                        self.frames[name] = frame
                        self.cams[name].putFrame(frame)
                    else:
                        print(f"[{name}] Failed to decode frame")
            except Exception as e:
                print(f"[{name}] Connection lost: {e}. Retrying in {RECONNECT_DELAY}s...")
                time.sleep(RECONNECT_DELAY)


class MyRobot(wpilib.TimedRobot):
    def robotInit(self):
        # --- Field / Pose ---
        self.field = Field2d()
        SmartDashboard.putData("Field", self.field)
        self.velocity_arrow = self.field.getObject("VelocityArrow")
        self.trajectory_points = []

        self.latest_pose = None
        self.gyro_offset_deg = 0.0
        self.last_wpilib_x = None
        self.last_wpilib_y = None
        self.last_time = None

        # --- Cameras ---
        self.multi_cam = MultiCameraServer(CAMERA_CONFIGS)

        print("Robot initialized. Waiting for Unity pose and video feeds...")

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
