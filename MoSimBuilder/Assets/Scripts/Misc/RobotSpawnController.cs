using System;
using System.Collections;
using UnityEditor;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.InputSystem;
using Object = UnityEngine.Object;

public class RobotSpawnController : MonoBehaviour
{
    private int _gamemode;
    private int _cameraMode;
    private int _blueRobotIndex;
    private int _redRobotIndex;
    private bool _reversedCamera;

    public static bool isMultiplayer;
    public static bool sameAlliance;

    private GameObject[] _robotPrefabs = new GameObject[1];

    [SerializeField] private string robotName;
    
    private GameObject[] _blueCameras = new GameObject[10];
    private GameObject[] _redCameras = new GameObject[10];
    private GameObject[] _secondaryBlueCameras = new GameObject[5];
    private GameObject[] _secondaryRedCameras = new GameObject[5];

    private GameObject _cameraBorder;

    private Transform blueSpawn;
    private Transform secondaryBlueSpawn;
    private Transform redSpawn;
    private Transform secondaryRedSpawn;

    [SerializeField] private CameraMode cameraMode;
    [SerializeField] private Alliance alliance;

    private void Start()
    {
        _blueCameras[0] = transform.Find("Blue").transform.Find("Main").transform.Find("BluePanCam").gameObject;
        _blueCameras[1] = transform.Find("Blue").transform.Find("Main").transform.Find("BlueFollowCam").gameObject;
        _blueCameras[2] = transform.Find("Blue").transform.Find("Main").transform.Find("BlueCloseCam").gameObject;
        _blueCameras[3] = transform.Find("Blue").transform.Find("Main").transform.Find("BlueCloseCamReversed").gameObject;
        _blueCameras[4] = transform.Find("Blue").transform.Find("Main").transform.Find("BlueFollowFlippedCam").gameObject;
        _blueCameras[5] = transform.Find("Blue").transform.Find("Main").transform.Find("BluePanCamSplit").gameObject;
        _blueCameras[6] = transform.Find("Blue").transform.Find("Main").transform.Find("BlueFollowCamSplit").gameObject;
        _blueCameras[7] = transform.Find("Blue").transform.Find("Main").transform.Find("BlueCloseCamSplit").gameObject;
        _blueCameras[8] = transform.Find("Blue").transform.Find("Main").transform.Find("BlueCloseCamSplitReversed").gameObject;
        _blueCameras[9] = transform.Find("Blue").transform.Find("Main").transform.Find("BlueFollowFlippedSplitCam").gameObject;
        
        _redCameras[0] = transform.Find("Red").transform.Find("Main").transform.Find("RedPanCam").gameObject;
        _redCameras[1] = transform.Find("Red").transform.Find("Main").transform.Find("RedFollowCam").gameObject;
        _redCameras[2] = transform.Find("Red").transform.Find("Main").transform.Find("RedCloseCam").gameObject;
        _redCameras[3] = transform.Find("Red").transform.Find("Main").transform.Find("RedCloseCamReversed").gameObject;
        _redCameras[4] = transform.Find("Red").transform.Find("Main").transform.Find("RedFollowFlippedCam").gameObject;
        _redCameras[5] = transform.Find("Red").transform.Find("Main").transform.Find("RedPanCamSplit").gameObject;
        _redCameras[6] = transform.Find("Red").transform.Find("Main").transform.Find("RedFollowCamSplit").gameObject;
        _redCameras[7] = transform.Find("Red").transform.Find("Main").transform.Find("RedCloseCamSplit").gameObject;
        _redCameras[8] = transform.Find("Red").transform.Find("Main").transform.Find("RedCloseCamSplitReversed").gameObject;
        _redCameras[9] = transform.Find("Red").transform.Find("Main").transform.Find("RedFollowFlippedSplitCam").gameObject;
        
        _secondaryBlueCameras[0] = transform.Find("Blue").transform.Find("Secondary").transform.Find("BluePanCamSplit").gameObject;
        _secondaryBlueCameras[1] = transform.Find("Blue").transform.Find("Secondary").transform.Find("BlueFollowCamSplit").gameObject;
        _secondaryBlueCameras[2] = transform.Find("Blue").transform.Find("Secondary").transform.Find("BlueCloseCamSplit").gameObject;
        _secondaryBlueCameras[3] = transform.Find("Blue").transform.Find("Secondary").transform.Find("BlueCloseCamSplitReversed").gameObject;
        _secondaryBlueCameras[4] = transform.Find("Blue").transform.Find("Main").transform.Find("BlueFollowFlippedSplitCam").gameObject;
        
        _secondaryRedCameras[0] = transform.Find("Red").transform.Find("Secondary").transform.Find("RedPanCamSplit").gameObject;
        _secondaryRedCameras[1] = transform.Find("Red").transform.Find("Secondary").transform.Find("RedFollowCamSplit").gameObject;
        _secondaryRedCameras[2] = transform.Find("Red").transform.Find("Secondary").transform.Find("RedCloseCamSplit").gameObject;
        _secondaryRedCameras[3] = transform.Find("Red").transform.Find("Secondary").transform.Find("RedCloseCamSplitReversed").gameObject;
        _secondaryRedCameras[4] = transform.Find("Red").transform.Find("Main").transform.Find("RedFollowFlippedSplitCam").gameObject;
        
        _cameraBorder = GameObject.Find("GameManagement").transform.Find("GameGUI").transform.Find("CameraBorder").gameObject;
        
        blueSpawn = transform.Find("BlueRobotSpawn").transform;
        redSpawn = transform.Find("RedRobotSpawn").transform;
        
        secondaryBlueSpawn = transform.Find("Blue").Find("BlueRobotSpawnSec").transform;
        secondaryRedSpawn = transform.Find("Red").Find("RedRobotSpawnSec").transform;
        
        _robotPrefabs[0] = Resources.Load("Robots/"+robotName.ToString(), typeof(GameObject)) as GameObject;
        
        _cameraMode = cameraMode switch
        {
            CameraMode.DriverStation => 0,
            CameraMode.Third => 1,
            CameraMode.First => 2,
            CameraMode.FlippedFirst => 3,
            CameraMode.ThirdFlipped => 4,
            _ => _cameraMode
        };

        _cameraBorder.SetActive(false);

        _redRobotIndex = 0;
        _blueRobotIndex = 0;

        if (_redRobotIndex > 2)
        {
            _redRobotIndex += 1;
        } else if (PlayerPrefs.GetInt("redShotBlocker") == 1 && _redRobotIndex == 2)
        {
            _redRobotIndex += 1;
        }

        if (_blueRobotIndex > 2)
        {
            _blueRobotIndex += 1;
        }
        else if (PlayerPrefs.GetInt("blueShotBlocker") == 1 && _blueRobotIndex == 2)
        {
            _blueRobotIndex += 1;
        }

        switch (_gamemode)
        {
            case 1:
                isMultiplayer = true;
                sameAlliance = false;
                break;
            case 2:
                sameAlliance = true;
                isMultiplayer = false;
                break;
            default:
                sameAlliance = false;
                isMultiplayer = false;
                break;
        }

        HideAll();

        if (isMultiplayer)
        {
            _cameraBorder.SetActive(true);


            _robotPrefabs[_redRobotIndex].GetComponent<PlayerInput>().defaultControlScheme = "Controls 2";
            _robotPrefabs[_redRobotIndex].GetComponent<DriveController>().isRedRobot = true;
            _robotPrefabs[_redRobotIndex].tag = "RedPlayer";



            switch (_cameraMode)
            {
                case 0:
                    _robotPrefabs[_redRobotIndex].GetComponent<DriveController>().isFieldCentric = true;
                    _robotPrefabs[_redRobotIndex].GetComponent<DriveController>().startingReversed = true;
                    break;
                case 1:
                    {
                        _robotPrefabs[_redRobotIndex].GetComponent<DriveController>().isFieldCentric = true;
                        _robotPrefabs[_redRobotIndex].GetComponent<DriveController>().startingReversed = false;

                        break;
                    }
                case 2:
                    _robotPrefabs[_redRobotIndex].GetComponent<DriveController>().isFieldCentric = false;
                    _robotPrefabs[_redRobotIndex].GetComponent<DriveController>().startingReversed = true;
                    break;
                case 3:
                    _robotPrefabs[_redRobotIndex].GetComponent<DriveController>().isFieldCentric = false;
                    _robotPrefabs[_redRobotIndex].GetComponent<DriveController>().startingReversed = true;
                    break;
                case 4:
                    _robotPrefabs[_redRobotIndex].GetComponent<DriveController>().isFieldCentric = false;
                    _robotPrefabs[_redRobotIndex].GetComponent<DriveController>().startingReversed = true;
                    break;
            }

            Instantiate(_robotPrefabs[_redRobotIndex], redSpawn.position, redSpawn.rotation);
            _redCameras[_cameraMode + 5].SetActive(true);


            _robotPrefabs[_blueRobotIndex].GetComponent<PlayerInput>().defaultControlScheme = "Controls 1";
            _robotPrefabs[_blueRobotIndex].tag = "Player";
            _robotPrefabs[_blueRobotIndex].GetComponent<DriveController>().isRedRobot = false;

            switch (_cameraMode)
            {
                case 0:
                    _robotPrefabs[_blueRobotIndex].GetComponent<DriveController>().isFieldCentric = true;
                    _robotPrefabs[_blueRobotIndex].GetComponent<DriveController>().startingReversed = true;
                    break;
                case 1:
                    {
                        _robotPrefabs[_blueRobotIndex].GetComponent<DriveController>().isFieldCentric = true;
                        _robotPrefabs[_blueRobotIndex].GetComponent<DriveController>().startingReversed = false;

                        break;
                    }
                case 2:
                    _robotPrefabs[_blueRobotIndex].GetComponent<DriveController>().isFieldCentric = false;
                    _robotPrefabs[_blueRobotIndex].GetComponent<DriveController>().startingReversed = false;
                    break;
                case 3:
                    _robotPrefabs[_blueRobotIndex].GetComponent<DriveController>().isFieldCentric = false;
                    _robotPrefabs[_blueRobotIndex].GetComponent<DriveController>().startingReversed = false;
                    break;
                case 4:
                    _robotPrefabs[_blueRobotIndex].GetComponent<DriveController>().isFieldCentric = false;
                    _robotPrefabs[_blueRobotIndex].GetComponent<DriveController>().startingReversed = true;
                    break;
            }

            Instantiate(_robotPrefabs[_blueRobotIndex], blueSpawn.position, blueSpawn.rotation);
            _blueCameras[_cameraMode + 5].SetActive(true);
        }
        else if (sameAlliance)
        {
            _cameraBorder.SetActive(true);

            if (PlayerPrefs.GetString("alliance") == "red")
            {
                _robotPrefabs[_redRobotIndex].GetComponent<PlayerInput>().defaultControlScheme = "Controls 1";
                _robotPrefabs[_redRobotIndex].tag = "RedPlayer";
                _robotPrefabs[_redRobotIndex].GetComponent<DriveController>().isRedRobot = true;


                switch (_cameraMode)
                {
                    case 0:
                        _robotPrefabs[_redRobotIndex].GetComponent<DriveController>().isFieldCentric = true;
                        _robotPrefabs[_redRobotIndex].GetComponent<DriveController>().startingReversed = true;
                        break;
                    case 1:
                        {
                            _robotPrefabs[_redRobotIndex].GetComponent<DriveController>().isFieldCentric = true;
                            _robotPrefabs[_redRobotIndex].GetComponent<DriveController>().startingReversed = false;
                            break;
                        }
                    case 2:
                        _robotPrefabs[_redRobotIndex].GetComponent<DriveController>().isFieldCentric = false;
                        _robotPrefabs[_redRobotIndex].GetComponent<DriveController>().startingReversed = true;
                        break;
                    case 3:
                        _robotPrefabs[_redRobotIndex].GetComponent<DriveController>().isFieldCentric = false;
                        _robotPrefabs[_redRobotIndex].GetComponent<DriveController>().startingReversed = true;
                        break;
                    case 4:
                        _robotPrefabs[_redRobotIndex].GetComponent<DriveController>().isFieldCentric = false;
                        _robotPrefabs[_redRobotIndex].GetComponent<DriveController>().startingReversed = true;
                        break;
                }

                Instantiate(_robotPrefabs[_redRobotIndex], redSpawn.position, redSpawn.rotation);
                _redCameras[_cameraMode + 5].SetActive(true);


                _robotPrefabs[_blueRobotIndex].GetComponent<PlayerInput>().defaultControlScheme = "Controls 2";
                _robotPrefabs[_blueRobotIndex].tag = "RedPlayer2";
                _robotPrefabs[_blueRobotIndex].GetComponent<DriveController>().isRedRobot = true;


                switch (_cameraMode)
                {
                    case 0:
                        _robotPrefabs[_blueRobotIndex].GetComponent<DriveController>().isFieldCentric = true;
                        _robotPrefabs[_blueRobotIndex].GetComponent<DriveController>().startingReversed = true;
                        break;
                    case 1:
                        {
                            _robotPrefabs[_blueRobotIndex].GetComponent<DriveController>().isFieldCentric = true;
                            _robotPrefabs[_blueRobotIndex].GetComponent<DriveController>().startingReversed = false;
                            break;
                        }
                    case 2:
                        _robotPrefabs[_blueRobotIndex].GetComponent<DriveController>().isFieldCentric = false;
                        _robotPrefabs[_blueRobotIndex].GetComponent<DriveController>().startingReversed = true;
                        break;
                    case 3:
                        _robotPrefabs[_blueRobotIndex].GetComponent<DriveController>().isFieldCentric = false;
                        _robotPrefabs[_blueRobotIndex].GetComponent<DriveController>().startingReversed = true;
                        break;
                    case 4:
                        _robotPrefabs[_blueRobotIndex].GetComponent<DriveController>().isFieldCentric = false;
                        _robotPrefabs[_blueRobotIndex].GetComponent<DriveController>().startingReversed = true;
                        break;
                }


                Instantiate(_robotPrefabs[_blueRobotIndex], secondaryRedSpawn.position,
                    secondaryRedSpawn.rotation);
                _secondaryRedCameras[_cameraMode].SetActive(true);

            }
            else
            {
                _robotPrefabs[_blueRobotIndex].GetComponent<PlayerInput>().defaultControlScheme = "Controls 1";
                _robotPrefabs[_blueRobotIndex].tag = "Player";
                _robotPrefabs[_blueRobotIndex].GetComponent<DriveController>().isRedRobot = false;

                switch (_cameraMode)
                {
                    case 0:
                        _robotPrefabs[_blueRobotIndex].GetComponent<DriveController>().isFieldCentric = true;
                        _robotPrefabs[_blueRobotIndex].GetComponent<DriveController>().startingReversed = true;
                        break;
                    case 1:
                        {
                            _robotPrefabs[_blueRobotIndex].GetComponent<DriveController>().isFieldCentric = true;
                            _robotPrefabs[_blueRobotIndex].GetComponent<DriveController>().startingReversed = false;
                            break;
                        }
                    case 2:
                        _robotPrefabs[_blueRobotIndex].GetComponent<DriveController>().isFieldCentric = false;
                        _robotPrefabs[_blueRobotIndex].GetComponent<DriveController>().startingReversed = true;
                        break;
                    case 3:
                        _robotPrefabs[_blueRobotIndex].GetComponent<DriveController>().isFieldCentric = false;
                        _robotPrefabs[_blueRobotIndex].GetComponent<DriveController>().startingReversed = true;
                        break;
                    case 4:
                        _robotPrefabs[_blueRobotIndex].GetComponent<DriveController>().isFieldCentric = false;
                        _robotPrefabs[_blueRobotIndex].GetComponent<DriveController>().startingReversed = true;
                        break;
                }

                Instantiate(_robotPrefabs[_blueRobotIndex], blueSpawn.position, blueSpawn.rotation);
                _blueCameras[_cameraMode + 5].SetActive(true);


                _robotPrefabs[_redRobotIndex].GetComponent<PlayerInput>().defaultControlScheme = "Controls 2";
                _robotPrefabs[_redRobotIndex].tag = "Player2";
                _robotPrefabs[_redRobotIndex].GetComponent<DriveController>().isRedRobot = false;

                switch (_cameraMode)
                {
                    case 0:
                        _robotPrefabs[_redRobotIndex].GetComponent<DriveController>().isFieldCentric = true;
                        _robotPrefabs[_redRobotIndex].GetComponent<DriveController>().startingReversed = true;
                        break;
                    case 1:
                        {
                            _robotPrefabs[_redRobotIndex].GetComponent<DriveController>().isFieldCentric = true;
                            _robotPrefabs[_redRobotIndex].GetComponent<DriveController>().startingReversed = false;
                            break;
                        }
                    case 2:
                        _robotPrefabs[_redRobotIndex].GetComponent<DriveController>().isFieldCentric = false;
                        _robotPrefabs[_redRobotIndex].GetComponent<DriveController>().startingReversed = false;
                        break;
                    case 3:
                        _robotPrefabs[_redRobotIndex].GetComponent<DriveController>().isFieldCentric = false;
                        _robotPrefabs[_redRobotIndex].GetComponent<DriveController>().startingReversed = true;
                        break;
                    case 4:
                        _robotPrefabs[_redRobotIndex].GetComponent<DriveController>().isFieldCentric = false;
                        _robotPrefabs[_redRobotIndex].GetComponent<DriveController>().startingReversed = true;
                        break;
                }

                Instantiate(_robotPrefabs[_redRobotIndex], secondaryBlueSpawn.position,
                    secondaryBlueSpawn.rotation);
                _secondaryBlueCameras[_cameraMode].SetActive(true);
            }
        }
        else
        {
            //Set correct robots & cameras active
            if (PlayerPrefs.GetString("alliance") == "red")
            {
                _robotPrefabs[_redRobotIndex].GetComponent<PlayerInput>().defaultControlScheme = "Controls 1";
                _robotPrefabs[_redRobotIndex].tag = "RedPlayer";
                _robotPrefabs[_redRobotIndex].GetComponent<DriveController>().isRedRobot = true;



                if (_cameraMode == 0)
                {
                    _robotPrefabs[_redRobotIndex].GetComponent<DriveController>().startingReversed = true;
                    _robotPrefabs[_redRobotIndex].GetComponent<DriveController>().isFieldCentric = true;
                }
                else if (_cameraMode == 1)
                {
                    _robotPrefabs[_redRobotIndex].GetComponent<DriveController>().isFieldCentric = true;
                    _robotPrefabs[_redRobotIndex].GetComponent<DriveController>().startingReversed = false;
                }
                else if (_cameraMode == 2)
                {
                    _robotPrefabs[_redRobotIndex].GetComponent<DriveController>().isFieldCentric = false;
                    _robotPrefabs[_redRobotIndex].GetComponent<DriveController>().startingReversed = false;
                }
                else if (_cameraMode == 3)
                {
                    _robotPrefabs[_redRobotIndex].GetComponent<DriveController>().isFieldCentric = false;
                } else if (_cameraMode == 4)
                {
                    _robotPrefabs[_redRobotIndex].GetComponent<DriveController>().isFieldCentric = true;
                    _robotPrefabs[_redRobotIndex].GetComponent<DriveController>().startingReversed = true;
                }

                Instantiate(_robotPrefabs[_redRobotIndex], redSpawn.position, redSpawn.rotation);
                _redCameras[_cameraMode].SetActive(true);
            }
            else
            {
                _robotPrefabs[_blueRobotIndex].GetComponent<PlayerInput>().defaultControlScheme = "Controls 1";
                _robotPrefabs[_blueRobotIndex].tag = "Player";
                _robotPrefabs[_blueRobotIndex].GetComponent<DriveController>().isRedRobot = false;


                if (_cameraMode == 0)
                {
                    _robotPrefabs[_blueRobotIndex].GetComponent<DriveController>().startingReversed = true;
                    _robotPrefabs[_blueRobotIndex].GetComponent<DriveController>().isFieldCentric = true;
                }
                else if (_cameraMode == 1)
                {
                    _robotPrefabs[_blueRobotIndex].GetComponent<DriveController>().isFieldCentric = true;
                    _robotPrefabs[_blueRobotIndex].GetComponent<DriveController>().startingReversed = false;
                }
                else if (_cameraMode == 2)
                {
                    _robotPrefabs[_blueRobotIndex].GetComponent<DriveController>().isFieldCentric = false;
                }
                else if (_cameraMode == 3)
                {
                    _robotPrefabs[_blueRobotIndex].GetComponent<DriveController>().isFieldCentric = false;
                } else if (_cameraMode == 4)
                {
                    _robotPrefabs[_blueRobotIndex].GetComponent<DriveController>().isFieldCentric = true;
                    _robotPrefabs[_blueRobotIndex].GetComponent<DriveController>().startingReversed = true;
                }

                Instantiate(_robotPrefabs[_blueRobotIndex], blueSpawn.position, blueSpawn.rotation);
                _blueCameras[_cameraMode].SetActive(true);
            }
        }
    }

    private void HideAll()
    {
        foreach (var blueCamera in _blueCameras)
        {
            blueCamera.SetActive(false);
        }

        foreach (var redCamera in _redCameras)
        {
            redCamera.SetActive(false);
        }

        foreach (var blueCamera in _secondaryBlueCameras)
        {
            blueCamera.SetActive(false);
        }

        foreach (var redCamera in _secondaryRedCameras)
        {
            redCamera.SetActive(false);
        }
    }

    public void Respawn()
    {

        _cameraBorder.SetActive(false);

        _gamemode = PlayerPrefs.GetInt("gamemode");
        _cameraMode = PlayerPrefs.GetInt("cameraMode");
        _redRobotIndex = PlayerPrefs.GetInt("redRobotSettings");
        _blueRobotIndex = PlayerPrefs.GetInt("blueRobotSettings");
        _reversedCamera = PlayerPrefs.GetInt("InvertedCamera") == 1;

        if (_redRobotIndex > 2)
        {
            _redRobotIndex += 1;
        } else if (PlayerPrefs.GetInt("redShotBlocker") == 1 || _redRobotIndex == 2)
        {
            _redRobotIndex += 1;
        }

        if (_blueRobotIndex > 2)
        {
            _blueRobotIndex += 1;
        }
        else if (PlayerPrefs.GetInt("blueShotBlocker") == 1 && _blueRobotIndex == 2)
        {
            _blueRobotIndex += 1;
        }

        if (_cameraMode == 2 && _reversedCamera)
        {
            _cameraMode += 1;
        }

        switch (_gamemode)
        {
            case 1:
                isMultiplayer = true;
                sameAlliance = false;
                break;
            case 2:
                sameAlliance = true;
                isMultiplayer = false;
                break;
            default:
                sameAlliance = false;
                isMultiplayer = false;
                break;
        }

        HideAll();

        if (isMultiplayer)
        {
            _cameraBorder.SetActive(true);


            _robotPrefabs[_redRobotIndex].GetComponent<PlayerInput>().defaultControlScheme = "Controls 2";
            _robotPrefabs[_redRobotIndex].GetComponent<DriveController>().isRedRobot = true;
            _robotPrefabs[_redRobotIndex].tag = "RedPlayer";



            switch (_cameraMode)
            {
                case 0:
                    _robotPrefabs[_redRobotIndex].GetComponent<DriveController>().isFieldCentric = true;
                    _robotPrefabs[_redRobotIndex].GetComponent<DriveController>().startingReversed = true;
                    break;
                case 1:
                    {
                        _robotPrefabs[_redRobotIndex].GetComponent<DriveController>().isFieldCentric = true;

                        break;
                    }
                case 2:
                    _robotPrefabs[_redRobotIndex].GetComponent<DriveController>().isFieldCentric = false;
                    _robotPrefabs[_redRobotIndex].GetComponent<DriveController>().startingReversed = true;
                    break;
                case 3:
                    _robotPrefabs[_redRobotIndex].GetComponent<DriveController>().isFieldCentric = false;
                    _robotPrefabs[_redRobotIndex].GetComponent<DriveController>().startingReversed = true;
                    break;
            }

            Instantiate(_robotPrefabs[_redRobotIndex], redSpawn.position, redSpawn.rotation);
            _redCameras[_cameraMode + 4].SetActive(true);


            _robotPrefabs[_blueRobotIndex].GetComponent<PlayerInput>().defaultControlScheme = "Controls 1";
            _robotPrefabs[_blueRobotIndex].tag = "Player";
            _robotPrefabs[_blueRobotIndex].GetComponent<DriveController>().isRedRobot = false;

            switch (_cameraMode)
            {
                case 0:
                    _robotPrefabs[_blueRobotIndex].GetComponent<DriveController>().isFieldCentric = true;
                    _robotPrefabs[_blueRobotIndex].GetComponent<DriveController>().startingReversed = true;
                    break;
                case 1:
                    {
                        _robotPrefabs[_blueRobotIndex].GetComponent<DriveController>().isFieldCentric = true;

                        break;
                    }
                case 2:
                    _robotPrefabs[_blueRobotIndex].GetComponent<DriveController>().isFieldCentric = false;
                    _robotPrefabs[_blueRobotIndex].GetComponent<DriveController>().startingReversed = false;
                    break;
                case 3:
                    _robotPrefabs[_blueRobotIndex].GetComponent<DriveController>().isFieldCentric = false;
                    _robotPrefabs[_blueRobotIndex].GetComponent<DriveController>().startingReversed = false;
                    break;
            }

            Instantiate(_robotPrefabs[_blueRobotIndex], blueSpawn.position, blueSpawn.rotation);
            _blueCameras[_cameraMode + 4].SetActive(true);
        }
        else if (sameAlliance)
        {
            _cameraBorder.SetActive(true);

            if (PlayerPrefs.GetString("alliance") == "red")
            {
                _robotPrefabs[_redRobotIndex].GetComponent<PlayerInput>().defaultControlScheme = "Controls 1";
                _robotPrefabs[_redRobotIndex].tag = "RedPlayer";
                _robotPrefabs[_redRobotIndex].GetComponent<DriveController>().isRedRobot = true;


                switch (_cameraMode)
                {
                    case 0:
                        _robotPrefabs[_redRobotIndex].GetComponent<DriveController>().isFieldCentric = true;
                        _robotPrefabs[_redRobotIndex].GetComponent<DriveController>().startingReversed = !_robotPrefabs[_redRobotIndex].GetComponent<DriveController>().startingReversed;
                        break;
                    case 1:
                        {
                            _robotPrefabs[_redRobotIndex].GetComponent<DriveController>().isFieldCentric = true;
                            _robotPrefabs[_redRobotIndex].GetComponent<DriveController>().startingReversed = false;
                            break;
                        }
                    case 2:
                        _robotPrefabs[_redRobotIndex].GetComponent<DriveController>().isFieldCentric = false;
                        _robotPrefabs[_redRobotIndex].GetComponent<DriveController>().startingReversed = true;
                        break;
                    case 3:
                        _robotPrefabs[_redRobotIndex].GetComponent<DriveController>().isFieldCentric = false;
                        _robotPrefabs[_redRobotIndex].GetComponent<DriveController>().startingReversed = true;
                        break;
                }

                Instantiate(_robotPrefabs[_redRobotIndex], redSpawn.position, redSpawn.rotation);
                _redCameras[_cameraMode + 4].SetActive(true);


                _robotPrefabs[_blueRobotIndex].GetComponent<PlayerInput>().defaultControlScheme = "Controls 2";
                _robotPrefabs[_blueRobotIndex].tag = "RedPlayer2";
                _robotPrefabs[_blueRobotIndex].GetComponent<DriveController>().isRedRobot = true;


                switch (_cameraMode)
                {
                    case 0:
                        _robotPrefabs[_blueRobotIndex].GetComponent<DriveController>().isFieldCentric = true;
                        _robotPrefabs[_blueRobotIndex].GetComponent<DriveController>().startingReversed = !_robotPrefabs[_blueRobotIndex].GetComponent<DriveController>().startingReversed;
                        break;
                    case 1:
                        {
                            _robotPrefabs[_blueRobotIndex].GetComponent<DriveController>().isFieldCentric = true;
                            _robotPrefabs[_blueRobotIndex].GetComponent<DriveController>().startingReversed = false;
                            break;
                        }
                    case 2:
                        _robotPrefabs[_blueRobotIndex].GetComponent<DriveController>().isFieldCentric = false;
                        _robotPrefabs[_blueRobotIndex].GetComponent<DriveController>().startingReversed = true;
                        break;
                    case 3:
                        _robotPrefabs[_blueRobotIndex].GetComponent<DriveController>().isFieldCentric = false;
                        _robotPrefabs[_blueRobotIndex].GetComponent<DriveController>().startingReversed = true;
                        break;
                }


                Instantiate(_robotPrefabs[_blueRobotIndex], secondaryRedSpawn.position,
                    secondaryRedSpawn.rotation);
                _secondaryRedCameras[_cameraMode].SetActive(true);

            }
            else
            {
                _robotPrefabs[_blueRobotIndex].GetComponent<PlayerInput>().defaultControlScheme = "Controls 1";
                _robotPrefabs[_blueRobotIndex].tag = "Player";
                _robotPrefabs[_blueRobotIndex].GetComponent<DriveController>().isRedRobot = false;

                switch (_cameraMode)
                {
                    case 0:
                        _robotPrefabs[_blueRobotIndex].GetComponent<DriveController>().isFieldCentric = true;
                        _robotPrefabs[_blueRobotIndex].GetComponent<DriveController>().startingReversed = !_robotPrefabs[_blueRobotIndex].GetComponent<DriveController>().startingReversed;
                        break;
                    case 1:
                        {
                            _robotPrefabs[_blueRobotIndex].GetComponent<DriveController>().isFieldCentric = true;
                            _robotPrefabs[_blueRobotIndex].GetComponent<DriveController>().startingReversed = false;
                            break;
                        }
                    case 2:
                        _robotPrefabs[_blueRobotIndex].GetComponent<DriveController>().isFieldCentric = false;
                        _robotPrefabs[_blueRobotIndex].GetComponent<DriveController>().startingReversed = true;
                        break;
                    case 3:
                        _robotPrefabs[_blueRobotIndex].GetComponent<DriveController>().isFieldCentric = false;
                        _robotPrefabs[_blueRobotIndex].GetComponent<DriveController>().startingReversed = true;
                        break;
                }

                Instantiate(_robotPrefabs[_blueRobotIndex], blueSpawn.position, blueSpawn.rotation);
                _blueCameras[_cameraMode + 4].SetActive(true);


                _robotPrefabs[_redRobotIndex].GetComponent<PlayerInput>().defaultControlScheme = "Controls 2";
                _robotPrefabs[_redRobotIndex].tag = "Player2";
                _robotPrefabs[_redRobotIndex].GetComponent<DriveController>().isRedRobot = false;

                switch (_cameraMode)
                {
                    case 0:
                        _robotPrefabs[_redRobotIndex].GetComponent<DriveController>().isFieldCentric = true;
                        _robotPrefabs[_redRobotIndex].GetComponent<DriveController>().startingReversed = !_robotPrefabs[_redRobotIndex].GetComponent<DriveController>().startingReversed;
                        break;
                    case 1:
                        {
                            _robotPrefabs[_redRobotIndex].GetComponent<DriveController>().isFieldCentric = true;
                            _robotPrefabs[_redRobotIndex].GetComponent<DriveController>().startingReversed = false;
                            break;
                        }
                    case 2:
                        _robotPrefabs[_redRobotIndex].GetComponent<DriveController>().isFieldCentric = false;
                        _robotPrefabs[_redRobotIndex].GetComponent<DriveController>().startingReversed = true;
                        break;
                    case 3:
                        _robotPrefabs[_redRobotIndex].GetComponent<DriveController>().isFieldCentric = false;
                        _robotPrefabs[_redRobotIndex].GetComponent<DriveController>().startingReversed = true;
                        break;
                }

                Instantiate(_robotPrefabs[_redRobotIndex], secondaryBlueSpawn.position,
                    secondaryBlueSpawn.rotation);
                _secondaryBlueCameras[_cameraMode].SetActive(true);
            }
        }
        else
        {
            //Set correct robots & cameras active
            if (alliance == Alliance.Red)
            {
                _robotPrefabs[_redRobotIndex].GetComponent<PlayerInput>().defaultControlScheme = "Controls 1";
                _robotPrefabs[_redRobotIndex].tag = "RedPlayer";
                _robotPrefabs[_redRobotIndex].GetComponent<DriveController>().isRedRobot = true;



                if (_cameraMode == 0)
                {
                    _robotPrefabs[_redRobotIndex].GetComponent<DriveController>().startingReversed = !_robotPrefabs[_redRobotIndex].GetComponent<DriveController>().startingReversed;
                    _robotPrefabs[_redRobotIndex].GetComponent<DriveController>().isFieldCentric = true;
                }
                else if (_cameraMode == 1)
                {
                    _robotPrefabs[_redRobotIndex].GetComponent<DriveController>().isFieldCentric = true;
                }
                else if (_cameraMode == 2)
                {
                    _robotPrefabs[_redRobotIndex].GetComponent<DriveController>().isFieldCentric = false;
                }
                else if (_cameraMode == 3)
                {
                    _robotPrefabs[_redRobotIndex].GetComponent<DriveController>().isFieldCentric = false;
                }

                Instantiate(_robotPrefabs[_redRobotIndex], redSpawn.position, redSpawn.rotation);
                _redCameras[_cameraMode].SetActive(true);
            }
            else
            {
                _robotPrefabs[_blueRobotIndex].GetComponent<PlayerInput>().defaultControlScheme = "Controls 1";
                _robotPrefabs[_blueRobotIndex].tag = "Player";
                _robotPrefabs[_blueRobotIndex].GetComponent<DriveController>().isRedRobot = false;


                if (_cameraMode == 0)
                {
                    _robotPrefabs[_blueRobotIndex].GetComponent<DriveController>().startingReversed =
                        !_robotPrefabs[_blueRobotIndex].GetComponent<DriveController>().startingReversed;
                    _robotPrefabs[_blueRobotIndex].GetComponent<DriveController>().isFieldCentric = true;
                }
                else if (_cameraMode == 1)
                {
                    _robotPrefabs[_blueRobotIndex].GetComponent<DriveController>().isFieldCentric = true;
                }
                else if (_cameraMode == 2)
                {
                    _robotPrefabs[_blueRobotIndex].GetComponent<DriveController>().isFieldCentric = false;
                }
                else if (_cameraMode == 3)
                {
                    _robotPrefabs[_blueRobotIndex].GetComponent<DriveController>().isFieldCentric = false;
                }

                Instantiate(_robotPrefabs[_blueRobotIndex], blueSpawn.position, blueSpawn.rotation);
                _blueCameras[_cameraMode].SetActive(true);
            }
        }

        if (GameObject.FindGameObjectsWithTag("MainCamera") != null)
        {
            GameObject[] Cameras = GameObject.FindGameObjectsWithTag("MainCamera");

            foreach (GameObject Camera in Cameras)
            {
                if (Camera.GetComponent<CameraScript>() != null)
                {
                    Camera.GetComponent<CameraScript>().Reset();
                }

                if (Camera.GetComponent<CameraPan>() != null)
                {
                    Camera.GetComponent<CameraPan>().Restart();
                }
               
            }
        }
    }
}