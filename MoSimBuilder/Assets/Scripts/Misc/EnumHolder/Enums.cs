//Enums for tracking alliance color, gamestates, etc...

public enum Alliance
{
    Red,
    Blue
}

public enum GamePieces
{
    Note,
    Cone,
    Cube,
    Coral,
    Algea
}

public enum Direction
{
    forward,
    up
}

public enum TubeType
{
    oneXTwo,
    twoXTwo,
    oneXOne,
    oneXThree
}

public enum SpeedUnit
{
    ft,
    M,
}

public enum Units
{
    inch,
    centimerter,
    millimeter,
    meter
}

public enum ControlType
{
    toggle,
    hold,
    sequence,
    lastPressed
}

public enum IntakeType
{
    hold,
    always
}

public enum TransferType
{
    button,
    instant
}

public enum CameraMode
{
    DriverStation,
    Third,
    ThirdFlipped,
    First,
    FlippedFirst
}

public enum Buttons
{
    A,
    X,
    Y,
    B,
    Lt,
    Rt,
    Lb,
    Rb,
    DpadUp,
    DpadDown,
    DpadLeft,
    DpadRight
}

public enum DriveTrain
{
    Tank,
    HDrive,
    Swerve
}

public enum SpawnType
{
    RobotDetect,
    PieceThreshold
}

//Unused for now
public enum RobotSettings
{
    CitrusCircuits,
    WaltonRobotics,
    RamRodzRobotics,
    Valor,
    MechanicalAdvantage,
    Robonauts,
    JackInTheBot,
    OffseasonDemo,
    WCPCC, 
    Steampunk
}

public enum SourceMode
{
    Random,
    Left,
    Center,
    Right
}

public enum GameState
{
    Auto,
    Teleop,
    Endgame,
    End
}