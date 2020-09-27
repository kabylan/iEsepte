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
            Console.WriteLine("1) Удаление 2) Копирование 3) Перемещение");
            Console.Write("Введите номер или название функции: ");
            string function = Console.ReadLine();
            app.SetFunctionType(function);
            
            Console.WriteLine("Выбрана функция: " + app.functionTypeNum + ". " + app.functionTypeName);
            Console.WriteLine("\n");
            Console.Write("Подтвердите(да/нет): ");

            string confirm = Console.ReadLine();
            if (confirm.Contains("нет"))
            {
                Console.Write("\n\nЗавершение работы программы...");
                Console.WriteLine("\niEsepte. (c) Esepte 2020. http://esepte.com/iEsepte\n\n");

                return;
            }

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

                app.AddRecognized(filePath, label);
            }

            Process.Start("explorer.exe", app.selectedFolderPath);

            Console.WriteLine("\n");
            Console.WriteLine(startDateTime);
            string endDateTime = DateTime.Now.ToString("Время завершения: HH:mm:ss");
            Console.WriteLine(endDateTime);
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
        
        public void SetFunctionType(string inputLine)
        {
            if (inputLine == "1" || inputLine.Contains("Удаление"))
            {
                functionTypeNum = 1;
                functionTypeName = "Удаление";
            }
            if (inputLine == "2" || inputLine.Contains("Копирование"))
            {
                functionTypeNum = 2;
                functionTypeName = "Копирование";
            }
            if (inputLine == "3" || inputLine.Contains("Перемещение"))
            {
                functionTypeNum = 3;
                functionTypeName = "Перемещение";
            }

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
