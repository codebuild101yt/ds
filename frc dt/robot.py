import socket
import wpilib
from wpilib import SmartDashboard, Field2d
from wpimath.geometry import Pose2d, Rotation2d

# ===== CONFIG =====
UDP_IP = "127.0.0.1"
UDP_PORT = 5805

# Unity units → meters
UNITY_TO_METERS = 1.0  # adjust if Unity units are cm/inches
# ==================

# UDP socket (non-blocking)
sock = socket.socket(socket.AF_INET, socket.SOCK_DGRAM)
sock.bind((UDP_IP, UDP_PORT))
sock.setblocking(False)

print(f"Listening for UDP pose on {UDP_IP}:{UDP_PORT}")


class MyRobot(wpilib.TimedRobot):
    def robotInit(self):
        self.field = Field2d()
        SmartDashboard.putData("Field", self.field)

        self.latest_pose = None
        self.gyro_offset_deg = 0.0

        print("Robot init complete")

    def teleopPeriodic(self):
        # Read all pending UDP packets, keep the latest
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

        # --- UNIT SCALE ---
        unity_x *= UNITY_TO_METERS
        unity_z *= UNITY_TO_METERS

        # --- UNITY → WPILIB AXES ---
        # Unity: X right, Z forward
        # WPILib: X forward, Y right (mirrored so robot is on right side of blue wall)
        wpilib_x = -unity_x
        wpilib_y = -unity_z

        # Gyro zero on any button press
        joystick = wpilib.Joystick(0)
        for i in range(1, joystick.getButtonCount() + 1):
            if joystick.getRawButton(i):
                self.gyro_offset_deg = heading_deg
                break

        gyro_deg = (heading_deg - self.gyro_offset_deg) % 360.0
        SmartDashboard.putNumber("Gyro", gyro_deg)

        self.field.setRobotPose(
            Pose2d(
                wpilib_x,
                wpilib_y,
                Rotation2d.fromDegrees(-gyro_deg + 270),  # +270° to align with WPILib field
            )
        )

        # Debug (leave on until verified)
        print(
            f"POSE | Unity(x,z)=({unity_x:.2f},{unity_z:.2f}) "
            f"→ WPILib(x,y)=({wpilib_x:.2f},{wpilib_y:.2f}) "
            f"heading={gyro_deg:.1f}°"
        )


if __name__ == "__main__":
    wpilib.run(MyRobot)
