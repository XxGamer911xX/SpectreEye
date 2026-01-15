using System;
using System.Management;
using System.Security.Cryptography;
using System.Text;

public static class HwidGenerator
{
    public static string GetHwid()
    {
        string cpu = GetWmi("Win32_Processor", "ProcessorId");
        string disk = GetWmi("Win32_DiskDrive", "SerialNumber");
        string bios = GetWmi("Win32_BIOS", "SerialNumber");

        string raw = cpu + disk + bios;

        using (SHA256 sha = SHA256.Create())
        {
            byte[] hash = sha.ComputeHash(Encoding.UTF8.GetBytes(raw));
            return BitConverter.ToString(hash).Replace("-", "");
        }
    }

    static string GetWmi(string cls, string prop)
    {
        try
        {
            using (var searcher = new ManagementObjectSearcher($"SELECT {prop} FROM {cls}"))
            {
                foreach (var obj in searcher.Get())
                {
                    return obj[prop]?.ToString()?.Trim() ?? "";
                }
            }
        }
        catch { }
        return "";
    }
}
