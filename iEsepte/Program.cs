using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Diagnostics;
using iEsepte.MachineLearningNoticeProperty.Model;
using Microsoft.ML;

namespace iEsepte
{
    class Program
    {
        

        static void Main(string[] args)
        {
            Console.WriteLine("\niEsepte. (c) Esepte 2020. http://esepte.com/iEsepte\n");

            DataOfProgram app = new DataOfProgram();
            
            app.InitializeIntellectMachine();

            Console.Write("\n\nУкажите путь к директории: ");
            app.selectedFolderPath = Console.ReadLine();
            app.InitializeArrayWithFilesPaths();
            Console.WriteLine("Изображений: " + app.filesPaths.Length);
            Console.WriteLine("\n");

            Console.WriteLine("Функции:");
            Console.WriteLine("1) Удаление");
            Console.WriteLine("\n");
            Console.Write("Введите номер или название функции: ");
            string function = Console.ReadLine();
            app.SetFunctionType(function);
            
            Console.WriteLine("Выбрана функция: " + app.functionTypeNum + ". " + app.functionTypeName);
            //Console.WriteLine("\n");
            //Console.Write("Подтвердите(да/нет): ");

            //string confirm = Console.ReadLine();
            //if (confirm.Contains("нет"))
            //{
            //    Console.Write("\n\nЗавершение работы программы...");
            //    Console.WriteLine("\niEsepte. (c) Esepte 2020. http://esepte.com/iEsepte\n\n");

            //    return;
            //}

            Console.WriteLine("\n\n");
            Console.WriteLine("Классы фотографий:");
            Console.WriteLine(app.GetClasses());
            Console.WriteLine("\n");
            Console.Write("Введите номер или название класса для " + app.functionTypeName + ": ");
            string classInput = Console.ReadLine();
            app.SetClassType(classInput);

            Console.WriteLine("Выбран класс: " + app.classTypeNum + ". " + app.classTypeName);
            //Console.WriteLine("\n");
            //Console.Write("Подтвердите(да/нет): ");

            //confirm = Console.ReadLine();
            //if (confirm.Contains("нет"))
            //{
            //    Console.Write("\n\nЗавершение работы программы...");
            //    Console.WriteLine("\niEsepte. (c) Esepte 2020. http://esepte.com/iEsepte\n\n");

            //    return;
            //}
            
            Console.WriteLine("\n");
            Console.WriteLine("Классификация...");
            string startDateTime = DateTime.Now.ToString("Время начала: HH:mm:ss");
            Console.WriteLine(startDateTime);
            Console.WriteLine("\n");

            int i = 0;
            foreach (string filePath in app.filesPaths)
            {
                i += 1;
                string imageName = Path.GetFileName(filePath);
                string label = app.Recognize(filePath);

                string row = i + ")\t" + app.GetTypeRU(label);
                
                if (app.GetTypeRU(label) == "спам" || app.GetTypeRU(label) == "логотип" || app.GetTypeRU(label) == "спам АН")
                {
                    row += "\t\t\t" + imageName;
                } else
                {
                    row += "\t\t" + imageName;
                }

                Console.WriteLine(row);

                if (app.GetTypeRU(label) == app.classTypeName)
                {
                    app.WriteRow(filePath);
                    app.CopyToBackupFolder(filePath);
                    app.AddRecognized(filePath, label);
                }
            }
            Console.WriteLine("\n");
            Console.Write("\t" + app.imageTypes.Count + " изображений класса \"" + app.classTypeName + "\"");

            Process.Start("explorer.exe", app.backupFolderPath);

            Console.WriteLine("\n");
            Console.WriteLine(startDateTime);
            string endDateTime = DateTime.Now.ToString("Время завершения: HH:mm:ss");
            Console.WriteLine(endDateTime);
            //Console.WriteLine("\n\n");
            //Console.WriteLine("Внимание: Для отмены удаления файла, - в названии файла включите текст \"_backup_\"");

            Console.Write("Подтвердите выполнить " + app.functionTypeName + " (да/нет): ");

            string confirm = Console.ReadLine();
            if (confirm.Contains("нет"))
            {
                Console.Write("\n\nЗавершение работы программы...");
                Console.WriteLine("\niEsepte. (c) Esepte 2020. http://esepte.com/iEsepte\n\n");

                return;
            }


            Console.Write("\nВыполнение функции " + app.functionTypeName + "...");

            if (app.functionTypeNum == 1)
            {
                int n = 0;
                foreach (var image in app.imageTypes)
                {
                    n++;
                    File.Delete(image.ImagePath);
                }
                Console.WriteLine("Удалено " + n + " изображений");
            }


            Console.Write("\n\nЗавершение работы программы...");
            Console.WriteLine("\niEsepte. (c) Esepte 2020. http://esepte.com/iEsepte\n\n");
            Console.WriteLine("\n\n");

        }



    }

    class DataOfProgram
    {
        public string selectedFolderPath { get; set; }

        public string[] filesPaths = { "" };

        public List<ImageRecognized> imageTypes = new List<ImageRecognized>();

        public int functionTypeNum = -1;
        public string functionTypeName = "Не выбрано";

        public int classTypeNum = -1;
        public string classTypeName = "-";

        public string backupFolderPath = "BackupFolder";
        public string backupFilePath = "BackupFile.txt";


        public void DeleteFiles()
        {
            int n = 0;
            foreach (var image in imageTypes)
            {
                n++;
                File.Delete(image.ImagePath);
            }
            Console.WriteLine("Удалено " + n + " изображений");
        }

        public void ExecuteFunction()
        {
            if (functionTypeNum == 1)
            {
                DeleteFiles();
            }
        }

        public void CopyToBackupFolder(string filePath)
        {
            try
            {
                File.Copy(filePath, backupFolderPath + "/" + Path.GetFileName(filePath));
            } catch
            {
            }
        }

        public void WriteRow(string row)
        {
            using (StreamWriter sw = new StreamWriter(backupFilePath, true, Encoding.UTF8))
            {
                sw.WriteLine(row);
            }
        }

        public void SetClassType(string inputLine)
        {
            if (inputLine == "1" || inputLine.Contains("2d планировака"))
            {
                classTypeNum = 1;
                classTypeName = "2d планировака";
            }
            if (inputLine == "2" || inputLine.Contains("3d планировака"))
            {
                classTypeNum = 2;
                classTypeName = "3d планировака";
            }
            if (inputLine == "3" || inputLine.Contains("логотип"))
            {
                classTypeNum = 3;
                classTypeName = "логотип";
            }
            if (inputLine == "4" || inputLine.Contains("спам АН"))
            {
                classTypeNum = 4;
                classTypeName = "спам АН";
            }
            if (inputLine == "5" || inputLine.Contains("документ"))
            {
                classTypeNum = 5;
                classTypeName = "документ";
            }
            if (inputLine == "6" || inputLine.Contains("карта"))
            {
                classTypeNum = 6;
                classTypeName = "карта";
            }
            if (inputLine == "7" || inputLine.Contains("спам"))
            {
                classTypeNum = 7;
                classTypeName = "спам";
            }
            if (inputLine == "8" || inputLine.Contains("реальное"))
            {
                classTypeNum = 8;
                classTypeName = "реальное";
            }
        }
        public string GetClasses()
        {
            string result = " 1) 2d планировака\n 2) 3d планировака\n 3) логотип\n 4) спам АН\n";
            result += " 5) документ\n 6) карта\n 7) спам\n 8) реальное";
            return result;
        }

        public void SetFunctionType(string inputLine)
        {
            if (inputLine == "1" || inputLine.Contains("Удаление"))
            {
                functionTypeNum = 1;
                functionTypeName = "Удаление";
            }
            //if (inputLine == "2" || inputLine.Contains("Копирование"))
            //{
            //    functionTypeNum = 2;
            //    functionTypeName = "Копирование";
            //}
            //if (inputLine == "3" || inputLine.Contains("Перемещение"))
            //{
            //    functionTypeNum = 3;
            //    functionTypeName = "Перемещение";
            //}

        }

        public void AddRecognized(string imageName, string imageType)
        {
            imageTypes.Add(new ImageRecognized(imageName, imageType));
        }

        public void InitializeArrayWithFilesPaths()
        {
            filesPaths = Directory.GetFiles(selectedFolderPath, "*.*", SearchOption.AllDirectories)
                .Where(s => s.EndsWith(".jpg") || s.EndsWith(".png") || s.EndsWith(".jpeg")).ToArray();
        }

        public string Recognize(string imagePath)
        {

            ModelInput sampleData = new ModelInput()
            {
                ImageSource = imagePath,
            };
            var predictionResult = Predict(sampleData);

            return predictionResult.Prediction;
        }


        public string GetTypeRU(string Prediction)
        {
            string typeRU = "";
            switch (Prediction)
            {
                case "2d_plan":
                    typeRU = "2d планировака";
                    break;
                case "2d_plan_photo":
                    typeRU = "документ";
                    break;
                case "3d_plan":
                    typeRU = "3d планировака";
                    break;
                case "artwork":
                    typeRU = "логотип";
                    break;
                case "desk_bred_property":
                    typeRU = "спам АН";
                    break;
                case "desk_bred_text_and_photo":
                    typeRU = "спам";
                    break;
                case "document":
                    typeRU = "документ";
                    break;
                case "map":
                    typeRU = "карта";
                    break;
                case "poster":
                    typeRU = "спам";
                    break;
                case "real":
                    typeRU = "реальное";
                    break;
                default:
                    typeRU = "-";
                    break;
            }
            return typeRU;
        }

        // one time
        private static Lazy<PredictionEngine<ModelInput, ModelOutput>> PredictionEngine = null;
        public static string MLNetModelPath = Path.GetFullPath("MachineLearningNoticeProperty/ntsprmdl.zip");
        public void InitializeIntellectMachine()
        {
            MLContext mlContext = new MLContext();
            ITransformer mlModel = mlContext.Model.Load(MLNetModelPath, out var modelInputSchema);
            var predEngine = mlContext.Model.CreatePredictionEngine<ModelInput, ModelOutput>(mlModel);
            PredictionEngine = new Lazy<PredictionEngine<ModelInput, ModelOutput>>(predEngine);
        }
        private static ModelOutput Predict(ModelInput input)
        {
            ModelOutput result = PredictionEngine.Value.Predict(input);
            return result;
        }


        public DataOfProgram()
        {
            //ClearBackupFolderAndFile();
        }

        public void ClearBackupFolderAndFile()
        {
            foreach (var file in Directory.GetFiles(backupFolderPath))
            {
                File.Delete(file);
            }

            using (StreamWriter sw = new StreamWriter(backupFilePath, false, Encoding.UTF8))
            {
                sw.Write("");
            }
        }
    }

    public class ImageRecognized
    {
        public string ImagePath { get; set; }
        public string ImageType { get; set; }

        public ImageRecognized(string imagePath, string imageType)
        {
            this.ImagePath = imagePath;
            this.ImageType = imageType;
        }
    }
}
