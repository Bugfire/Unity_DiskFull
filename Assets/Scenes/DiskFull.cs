using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEngine;
using UnityEngine.UI;

public class DiskFull : MonoBehaviour
{
    public Text Log = null;

    private List<string> Messages = new List<string>();

    private Coroutine FillCoroutine = null;

    private long Serial = 0;
    private long WroteSize = 0;
    private string TmpDir = null;

    private void Start()
    {
        TmpDir = Path.Combine(Application.temporaryCachePath, "fill");
        AddLog(TmpDir);
        Debug.Log(TmpDir);
    }

    public void OnReadPrefs()
    {
        try
        {
            var prefTest = PlayerPrefs.GetString("test", null);
            AddLog("Pref => {0}", prefTest);
        }
        catch (Exception ex)
        {
            AddLog("ReadPrefs Exception");
            AddException(ex);
        }
    }

    public void OnWritePrefs()
    {
        try
        {
            var b = new StringBuilder();
            for (var i = 0; i < 16; i++)
            {
                b.Append((Char)UnityEngine.Random.Range(0x20, 0x40));
            }
            var r = b.ToString();
            PlayerPrefs.SetString("test", r);
            AddLog("Pref Write <= {0}", r);
        }
        catch (Exception ex)
        {
            AddLog("WritePrefs Exception");
            AddException(ex);
        }
    }

    public void OnFill()
    {
        if (FillCoroutine != null)
        {
            AddLog("Fill Stopped wrote {0:#,0}bytes", WroteSize);
            StopCoroutine(FillCoroutine);
            FillCoroutine = null;
            return;
        }
        else
        {
            AddLog("Fill Start");
            Directory.CreateDirectory(TmpDir);
            FillCoroutine = StartCoroutine(Fill(TmpDir));
        }
    }

    public void OnClear()
    {
        AddLog("Clear");
        Directory.Delete(TmpDir, true);
    }

    private IEnumerator Fill(string tmpDir)
    {
        var sizes = new int[] { 1024 * 1024 * 64, 1024 * 1024, 1024, 1 };
        WroteSize = 0;
        for (var i = 0; i < sizes.Length; i++)
        {
            var unitSize = sizes[i];
            AddLog("Filling unit {0:#,0}bytes", unitSize);
            var dummy = new byte[unitSize];
            while (true)
            {
                var p = Path.Combine(tmpDir, Serial.ToString());
                Serial++;
                try
                {
                    File.WriteAllBytes(p, dummy);
                    WroteSize += unitSize;
                }
                catch (Exception ex)
                {
                    AddException(ex);
                    break;
                }
                yield return null;
            }
        }
    }

    private void AddLog(string format, params object[] args)
    {
        AddLogString(string.Format(format, args));
    }

    private void AddException(Exception ex)
    {
        AddLogString(ex.ToString());
    }

    private void AddLogString(string message)
    {
        Messages.Add(message);
        if (Messages.Count > 20)
        {
            Messages.RemoveAt(0);
        }
        Log.text = string.Join("\n", Messages);
    }
}
