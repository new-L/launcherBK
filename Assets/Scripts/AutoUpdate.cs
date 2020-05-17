using System.Collections;
using System.IO;
using UnityEngine;
using UnityEngine.UI;
using System.Diagnostics;
using System.Net;
using System;
using System.ComponentModel;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

public class AutoUpdate : MonoBehaviour
{
    private readonly static string url = "https://becomingking.ru/version_controll/version_controll.txt";
    private readonly static string gameURL = "https://becomingking.ru/version_controll/setup.exe";
    private const string gameFolder = "BecomingKing";
    [SerializeField]
    private Text m_Precentage,
        m_CurrentVersion;
    [SerializeField] private Button m_Updater;
    [SerializeField] private Slider m_ProgressBar;
    [SerializeField] private Image _Fill;
    private string serverVersion = "";
    private WebClient client;
    private TextAnimatiopn animation;
    private static bool IsUpdated = false;
    private static bool IsSetuped = false;
    private IEnumerator Start()
    {
        WWW update = new WWW(url);
        yield return update;
        serverVersion = update.text;
        CreateFile();
        m_CurrentVersion.text = ReadVersionFile();
        if (serverVersion.Equals(ReadVersionFile()))
        {
            if (CheckSetup())
            {
                IsUpdated = true;
                IsSetuped = true;
                m_Precentage.text = "Вы обновлены до последней версии, приятной игры!";
                m_Updater.GetComponentInChildren<Text>().text = "играть".ToUpper();
            }
            else
            {
                m_Updater.enabled = true;
                IsUpdated = true;
                IsSetuped = false;
                m_Precentage.text = "Требуется установка!";
                m_Updater.GetComponentInChildren<Text>().text = "Установить".ToUpper();
            }
            m_ProgressBar.value = 100;
            _Fill.sprite = Resources.Load<Sprite>("progress bar_Updated");
        }
        else
        {
            IsSetuped = false;
            IsUpdated = false;
            m_Precentage.text = "Доступная новая версия игры! " + serverVersion;
            m_Updater.GetComponentInChildren<Text>().text = "Обновить".ToUpper();
        }
    }
    public void CheckUpdate()
    {
        if (IsUpdated && IsSetuped) GoToPlay();
        else if (!IsUpdated && !IsSetuped) GoToDownload();
        else if (IsUpdated && !IsSetuped) Setup();
    } 

    private void GoToPlay()
    {
        OpenGame();
       // m_Updater.enabled = false;
        
    }

    private void GoToDownload()
    {
        RenameOldVersion();
        DownloadGame();
        m_Updater.enabled = false;
        animation = FindObjectOfType<TextAnimatiopn>();
        animation.OnUpdate();
    }

    //Скачивание игры
    private bool CheckTheGame()
    {
        return Directory.Exists(Application.dataPath + "/" + gameFolder + "/");
        
    }

    private void RenameOldVersion()
    {
        if(Directory.Exists(Application.dataPath + "/" + gameFolder + "_old/")) Directory.Delete(Application.dataPath + "/" + gameFolder + "_old/");
        if(Directory.Exists(Application.dataPath + "/" + "BeKingSetup.exe")) Directory.Delete(Application.dataPath + "/" + "BeKingSetup.exe");
        //Если у нас есть старая версия, переименовываем для подстраховки, после проверки на работоспособность новой версии - удалить
        if (CheckTheGame())
        {
            Directory.Move(Application.dataPath + "/" + gameFolder + "/", Application.dataPath + "/" + gameFolder + "_old/");
        }

    }
    
    private void DownloadGame()
    {
        using (client= new WebClient())
        {
            client.DownloadFileCompleted += new AsyncCompletedEventHandler(Completed);
            client.DownloadProgressChanged += new DownloadProgressChangedEventHandler(ProgressChanged);
            try
            {
                client.DownloadFileAsync(new Uri(gameURL), Application.dataPath + "/" + "BeKingSetup.exe");
            }catch(Exception e)
            {
                print(e);
                client.CancelAsync();
            }
        }
    }

    private void ProgressChanged(object sender, DownloadProgressChangedEventArgs e)
    {
        if (((double)e.BytesReceived / 1048576) < 1)
        {
            m_Precentage.text = $"Загружено: {e.ProgressPercentage}% ({((double)e.BytesReceived / 1048576).ToString("0.# МБ")})";
        }
        else
        {
            m_Precentage.text = $"Загружено: {e.ProgressPercentage}% ({((double)e.BytesReceived / 1048576).ToString("#.# МБ")})";
        }
        m_ProgressBar.value = e.ProgressPercentage;
    }

    private void Completed(object sender, AsyncCompletedEventArgs e)
    {
        if (e.Error != null) print(e.Error);
        else
        {
            WriteVersionFile(serverVersion);
            animation.SetCurrentVersion(serverVersion);
            StartCoroutine(Start());
        }
    }

    //Установка игры
    private async void Setup()
    {
        await Task.Run(() =>
        {
            try
            {
                m_Updater.GetComponentInChildren<Text>().text = "Установка".ToUpper();
                m_Updater.GetComponent<Image>().color = new Color32(147,147,147,255);
                string drive = Application.dataPath.Substring(0, Application.dataPath.IndexOf(":"));
                string batPath = Path.Combine(Path.GetTempPath(), "abc.bat");
                string commands = @"@echo off
                                echo Please wait, program installing
                                cd " + Application.dataPath + "\n"
                                    + drive + ":\n"
                                    + "start /wait BeKingSetup.exe /VERYSILENT /DIR=" + Application.dataPath + "\\BecomingKing /SUPPRESSMSGBOXES /NORESTART\n"
                                    + "exit";
                File.WriteAllText(batPath, commands, Encoding.Default);
                Process cmd = new Process();
                cmd.StartInfo.FileName = batPath;
                cmd.StartInfo.WindowStyle = ProcessWindowStyle.Hidden;
                cmd.Start();
                cmd.WaitForExit();
                File.Delete(batPath);
                IsSetuped = true;
            }
            catch (Exception e)
            {
                print(e);
            }
        });
        
    }

    private bool CheckSetup()
    {
        return Directory.Exists(Application.dataPath + "\\BecomingKing\\BecomingKing.exe");
    }
    private void Update()
    {
        if (IsSetuped)
        {
            m_Precentage.text = "Вы обновлены до последней версии, приятной игры!";
            m_Updater.GetComponentInChildren<Text>().text = "Играть".ToUpper();
            m_Updater.GetComponent<Image>().color = new Color32(255, 255, 255, 255);
            m_Updater.enabled = true;
        }
    }

    private void OpenGame()
    {

        try
        {
            string drive = Application.dataPath.Substring(0, Application.dataPath.IndexOf(":"));
            string batPath = Path.Combine(Path.GetTempPath(), "abc.bat");
            string commands = @"@echo off
                                echo Please wait, program installing
                                cd " + Application.dataPath + "\n"
                                + drive + ":\n"
                                + "start " + Application.dataPath + "\\BecomingKing\\BecomingKing.exe "
                                + "exit";
            File.WriteAllText(batPath, commands, Encoding.Default);
            Process cmd = new Process();
            cmd.StartInfo.FileName = batPath;
            cmd.StartInfo.WindowStyle = ProcessWindowStyle.Hidden;
            cmd.Start();
            cmd.WaitForExit();
            File.Delete(batPath);
        }
        catch (Exception e)
        {
            print(e);
        }
    }


    //Работа с файлами версии
    private void CreateFile()
    {
        Directory.CreateDirectory(Application.dataPath + "/VersionData/");
        FileInfo versionText = new FileInfo(Application.dataPath + "/VersionData/current_version.txt");
        if (!versionText.Exists) versionText.Create().Dispose();
    }

    private string ReadVersionFile()
    {
        StreamReader streamReader = new StreamReader(Application.dataPath + "/VersionData/current_version.txt");
        string currentVersion = "";
        while (!streamReader.EndOfStream)
        {
            currentVersion += streamReader.ReadLine();
        }
        streamReader.Close();
        return currentVersion;
    }

    private void WriteVersionFile(string version)
    {
        StreamWriter streamWriter = new StreamWriter(Application.dataPath + "/VersionData/current_version.txt");
        streamWriter.Write(version);
        streamWriter.Close();
    }


    private void OnDestroy()
    {
        if(client != null)client.CancelAsync();
    }
}
