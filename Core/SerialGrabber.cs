using Microsoft.Win32;
using System;

public class SerialGrabber
{
    public static string GetMTASerial()
    {
        string[] paths = new string[]
        {
            @"SOFTWARE\WOW6432Node\Multi Theft Auto: San Andreas All\1.6\Settings\general",
            @"SOFTWARE\Multi Theft Auto: San Andreas All\1.6\Settings\general",
            @"SOFTWARE\WOW6432Node\Multi Theft Auto: San Andreas All\1.5\Settings\general",
            @"SOFTWARE\Multi Theft Auto: San Andreas All\1.5\Settings\general"
        };

        foreach (string path in paths)
        {
            try
            {
                using (RegistryKey key = Registry.LocalMachine.OpenSubKey(path))
                {
                    if (key != null)
                    {
                        object serial = key.GetValue("serial");
                        if (serial != null)
                            return serial.ToString();
                    }
                }
            }
            catch { }
        }

        return null;
    }
}
