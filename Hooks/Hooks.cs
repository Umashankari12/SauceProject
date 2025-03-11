// using NUnit.Framework;
// using OpenQA.Selenium;
// using TechTalk.SpecFlow;
// using WebDriverManager.DriverConfigs.Impl;
// using WebDriverManager;
// using AventStack.ExtentReports.Reporter;
// using AventStack.ExtentReports;
// using System;
// using System.IO;
// using System.Threading;
// using OpenQA.Selenium.Chrome;

// namespace SwagProject.Hooks
// {
//     [Binding]
//     public class Hooks
//     {
//         public static IWebDriver driver;
//         private readonly ScenarioContext _scenarioContext;
//         private static ExtentReports _extent;
//         private static ExtentTest _feature;
//         private ExtentTest _scenario;
//         private static ExtentSparkReporter _sparkReporter;

//         public Hooks(ScenarioContext scenarioContext)
//         {
//             _scenarioContext = scenarioContext;
//         }

//         [BeforeTestRun]
//         public static void BeforeTestRun()
//         {
//             string reportDirectory = Path.Combine(Directory.GetCurrentDirectory(), "Artifacts", "Reports");
//             string reportPath = Path.Combine(reportDirectory, "ExtentReport.html");
//             Directory.CreateDirectory(reportDirectory);

//             _sparkReporter = new ExtentSparkReporter(reportPath);
//             _extent = new ExtentReports();
//             _extent.AttachReporter(_sparkReporter);
//         }

//         [BeforeFeature]
//         public static void BeforeFeature(FeatureContext featureContext)
//         {
//             _feature = _extent.CreateTest(featureContext.FeatureInfo.Title);
//         }

//         [BeforeScenario]
//         public void Setup()
//         {
//             TestContext.Progress.WriteLine("Initializing WebDriver in headless mode...");

//             new DriverManager().SetUpDriver(new ChromeConfig());
//             ChromeOptions chromeOptions = new ChromeOptions();
            
//             // Headless Mode & Additional Arguments for CI Environments
//             chromeOptions.AddArgument("--headless=new");
//             chromeOptions.AddArgument("--disable-gpu");
//             chromeOptions.AddArgument("--no-sandbox");
//             chromeOptions.AddArgument("--window-size=1920,1080");

//             // Set Chrome Binary Path explicitly
//             chromeOptions.BinaryLocation = "/usr/bin/google-chrome";

//             driver = new ChromeDriver(chromeOptions);
            
//             _scenarioContext["WebDriver"] = driver;
//             _scenario = _feature.CreateNode(_scenarioContext.ScenarioInfo.Title);
//         }

//         [AfterScenario]
//         public void TearDown()
//         {
//             if (driver != null)
//             {
//                 driver.Quit();
//                 driver.Dispose();
//                 driver = null;
//             }
//         }

//         [AfterTestRun]
//         public static void AfterTestRun()
//         {
//             _extent.Flush();
//         }
//     }
// }


using NUnit.Framework;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using System;
using System.IO;
using System.Net;
using System.Net.Mail;
using System.Threading;
using TechTalk.SpecFlow;
using AventStack.ExtentReports;
using AventStack.ExtentReports.Reporter;

namespace SwagProject.Hooks
{
    [Binding]
    public class Hooks
    {
        private static IWebDriver driver;
        private static ExtentReports _extent;
        private static ExtentTest _feature;
        private ExtentTest _scenario;
        private static ExtentSparkReporter _sparkReporter;
        private static string reportPath;
        private static string screenshotsDir;
        private readonly ScenarioContext _scenarioContext;

        public Hooks(ScenarioContext scenarioContext)
        {
            _scenarioContext = scenarioContext;
        }

        [BeforeTestRun]
        public static void BeforeTestRun()
        {
            string reportDirectory = Path.Combine(Directory.GetCurrentDirectory(), "Reports");
            reportPath = Path.Combine(reportDirectory, "ExtentReport.html");
            screenshotsDir = Path.Combine(reportDirectory, "Screenshots");

            // Ensure Reports Directory Exists
            Directory.CreateDirectory(reportDirectory);
            Directory.CreateDirectory(screenshotsDir);

            _sparkReporter = new ExtentSparkReporter(reportPath);
            _extent = new ExtentReports();
            _extent.AttachReporter(_sparkReporter);
        }

        [BeforeScenario]
        public void BeforeScenario()
        {
            _scenario = _feature.CreateNode<Scenario>(_scenarioContext.ScenarioInfo.Title);

            if (driver == null)
            {
                ChromeOptions options = new ChromeOptions();

                // Specify the Chrome binary path (for CI environments)
                options.BinaryLocation = "/usr/bin/google-chrome";

                // Run Chrome in headless mode (CI/CD friendly)
                options.AddArgument("--headless");
                options.AddArgument("--no-sandbox");
                options.AddArgument("--disable-dev-shm-usage");

                driver = new ChromeDriver(options);
            }

            _scenarioContext["WebDriver"] = driver;
        }

        [AfterScenario]
        public void AfterScenario()
        {
            if (_scenarioContext.TestError != null)
            {
                string scenarioName = _scenarioContext.ScenarioInfo.Title;
                string stepName = _scenarioContext.StepContext.StepInfo.Text;
                string screenshotPath = CaptureScreenshot(scenarioName, stepName);

                if (!string.IsNullOrEmpty(screenshotPath))
                {
                    _scenario.AddScreenCaptureFromPath(screenshotPath);
                }
            }
        }

        [AfterTestRun]
        public static void AfterTestRun()
        {
            _extent.Flush();
            SendEmailWithGmail();
        }

        private string CaptureScreenshot(string scenarioName, string stepName)
        {
            try
            {
                if (driver == null)
                {
                    return null;
                }

                Thread.Sleep(500); // Small wait before capturing screenshot
                Screenshot screenshot = ((ITakesScreenshot)driver).GetScreenshot();

                // Generate a safe filename
                string sanitizedStepName = string.Join("_", stepName.Split(Path.GetInvalidFileNameChars()));
                string fileName = $"{scenarioName}_{sanitizedStepName}.png";
                string filePath = Path.Combine(screenshotsDir, fileName);

                screenshot.SaveAsFile(filePath);
                TestContext.Progress.WriteLine($"Screenshot saved: {filePath}");

                return Path.Combine("Screenshots", fileName); // Return relative path for Extent Report
            }
            catch (Exception ex)
            {
                TestContext.Progress.WriteLine($"Failed to capture screenshot: {ex.Message}");
                return null;
            }
        }

        private static void SendEmailWithGmail()
        {
            try
            {
                string smtpServer = "smtp.gmail.com";
                int smtpPort = 587;
                string senderEmail = "shankariu804@gmail.com"; // Replace with your Gmail address
                string senderPassword = "exry tjbv yrxb ctnu"; // Use the App Password (16 characters)
                string recipientEmail = "shankariu8@gmail.com";

                MailMessage mail = new MailMessage
                {
                    From = new MailAddress(senderEmail),
                    Subject = "SpecFlow Test Report & Screenshots",
                    Body = "Attached are the Extent Report and failure screenshots from the latest test execution.",
                    IsBodyHtml = false
                };

                mail.To.Add(recipientEmail);

                // Attach Extent Report
                if (File.Exists(reportPath))
                {
                    mail.Attachments.Add(new Attachment(reportPath));
                }

                // Attach Screenshots (if any)
                foreach (string screenshot in Directory.GetFiles(screenshotsDir, "*.png"))
                {
                    mail.Attachments.Add(new Attachment(screenshot));
                }

                SmtpClient smtp = new SmtpClient(smtpServer, smtpPort)
                {
                    Credentials = new NetworkCredential(senderEmail, senderPassword),
                    EnableSsl = true
                };

                smtp.Send(mail);
                TestContext.Progress.WriteLine("✅ Email sent successfully via Gmail SMTP!");
            }
            catch (Exception ex)
            {
                TestContext.Progress.WriteLine($"❌ Failed to send email via Gmail SMTP: {ex.Message}");
            }
        }
    }
}
