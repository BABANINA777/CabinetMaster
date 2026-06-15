using System;
using System.IO;
using System.Threading.Tasks;

namespace CabinetMaster.Services;

public class BackupService
{
    private readonly string BackUpfolderName = "Backup";
    private readonly string DBaseFileName = "cabinetmaster.db";

    public void RunBackupIfNeeded()
    {
        if (!File.Exists(DBaseFileName)) return;
        if (!Directory.Exists(BackUpfolderName)) Directory.CreateDirectory(BackUpfolderName);

        // 1. Берем ТОЛЬКО файлы баз данных
        var backups = new DirectoryInfo(BackUpfolderName).GetFiles("*.db");

        // 2. Ищем дату последнего бэкапа
        DateTime lastdatefile = DateTime.MinValue;
        foreach (var fileInfo in backups)
        {
            if (lastdatefile < fileInfo.CreationTime)
            {
                lastdatefile = fileInfo.CreationTime;
            }
        }

        // 3. Определяем, сколько дней нужно ждать, в зависимости от количества файлов
        int daysToWait;
        if (backups.Length < 3)
        {
            daysToWait = 1; // Первые 3 файла - каждый день
        }
        else if (backups.Length < 5)
        {
            daysToWait = 2; // 4-й и 5-й файлы - раз в 2 дня
        }
        else
        {
            daysToWait = 7; // Дальше - раз в неделю
        }

        // 4. Делаем одну финальную проверку
        // Если прошло нужное количество дней ИЛИ файлов еще вообще нет
        if (lastdatefile == DateTime.MinValue || DateTime.Now >= lastdatefile.AddDays(daysToWait))
        {
            CreateBackup();
        }
    }

    private void CreateBackup()
    {
        string DateForName = DateTime.Now.ToString("yyyyMMddHHmmss");
        string newFileName = $"backup_{DateForName}.db";
        string targetPath = Path.Combine(BackUpfolderName, newFileName);
        File.Copy(DBaseFileName, targetPath, true);

        File.SetCreationTime(targetPath, DateTime.Now);
    }
}