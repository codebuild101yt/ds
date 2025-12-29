import wpilib
from wpilib import SmartDashboard, Field2d
import pygame
import elasticlib
import math
from wpimath.geometry import Pose2d, Rotation2d

CSV_PATH = r"c:\Users\Innov\AppData\LocalLow\GreenMachineStudios\MoSimKrayonCad\robot_pose.csv"

pygame.init()
pygame.joystick.init()

pg_joystick = None
if pygame.joystick.get_count() > 0:
    pg_joystick = pygame.joystick.Joystick(0)
    pg_joystick.init()
    print(f"Pygame joystick detected: {pg_joystick.get_name()}")


class MyRobot(wpilib.TimedRobot):
    def robotInit(self):
        self.joystick = wpilib.Joystick(0)

        # Gyro state
        self.gyro_offset_deg = 0.0
        self.gyro_deg = 0.0

        # Field display
        self.field = Field2d()
        SmartDashboard.putData("Field", self.field)

        # CSV (open once)
        self.csv_file = open(CSV_PATH, "r")
        self.csv_file.seek(0, 2)

        elasticlib.send_notification(
            elasticlib.Notification(
                level=elasticlib.NotificationLevel.INFO,
                title="Robot Code Started",
                description="Gyro + Field2d from MoSim CSV",
            )
        )

        print("Robot Init complete")

    def read_csv_pose(self):
        """
        Returns (x_m, y_m, heading_deg) or None
        """
        line = self.csv_file.readline()
        if not line:
            return None

        try:
            _, x, y, heading_rad = line.strip().split(",")
            heading_deg = math.degrees(float(heading_rad))
            return float(x), float(y), heading_deg
        except ValueError:
            return None

    def teleopPeriodic(self):
        reset = False

        # --- WPILib buttons (1-indexed) ---
        for i in range(1, self.joystick.getButtonCount() + 1):
            if self.joystick.getRawButton(i):
                reset = True
                break

        # --- Pygame buttons ---
        if pg_joystick:
            pygame.event.pump()
            for i in range(pg_joystick.get_numbuttons()):
                if pg_joystick.get_button(i):
                    reset = True
                    break

        # --- Read pose from CSV ---
        pose = self.read_csv_pose()
        if pose:
            x_m, y_m, heading_deg = pose

            if reset:
                self.gyro_offset_deg = heading_deg
                print("Gyro zeroed")

            # Gyro value
            self.gyro_deg = (heading_deg - self.gyro_offset_deg) % 360.0

            # Publish gyro
            SmartDashboard.putNumber("Gyro", self.gyro_deg)

            # Publish Field2d pose
            self.field.setRobotPose(
                Pose2d(x_m, y_m, Rotation2d.fromDegrees(self.gyro_deg))
            )

            print(
                f"Pose → x={x_m:.2f} m, y={y_m:.2f} m, gyro={self.gyro_deg:.2f}°"
            )


if __name__ == "__main__":
    wpilib.run(MyRobot)
