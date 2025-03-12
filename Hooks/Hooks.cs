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
using OpenQA.Selenium.Firefox;
using OpenQA.Selenium;
using TechTalk.SpecFlow;
using WebDriverManager.DriverConfigs.Impl;
using WebDriverManager;
using AventStack.ExtentReports.Reporter;
using AventStack.ExtentReports;
using System;
using System.IO;
using System.Net;
using System.Net.Mail;  // ✅ Add this to support email functionality
using System.Threading;
using OpenQA.Selenium.Chrome;

namespace SwagProject.Hooks
{
    [Binding]
    public class Hooks
    {
        public static IWebDriver driver;
        private readonly ScenarioContext _scenarioContext;
        private static ExtentReports _extent;
        private static ExtentTest _feature;
        private ExtentTest _scenario;
        private static ExtentSparkReporter _sparkReporter;

        public Hooks(ScenarioContext scenarioContext)
        {
            _scenarioContext = scenarioContext;
        }

        [BeforeTestRun]
        public static void BeforeTestRun()
        {
            string reportDirectory = Path.Combine(Directory.GetCurrentDirectory(), "Reports");
            string reportPath = Path.Combine(reportDirectory, "ExtentReport.html");

            // ✅ Ensure Reports Directory Exists
            Directory.CreateDirectory(reportDirectory);

            _sparkReporter = new ExtentSparkReporter(reportPath);
            _extent = new ExtentReports();
            _extent.AttachReporter(_sparkReporter);
        }

        [BeforeFeature]
        public static void BeforeFeature(FeatureContext featureContext)
        {
            _feature = _extent.CreateTest(featureContext.FeatureInfo.Title);
        }

        [BeforeScenario]
        public void Setup()
        {
            TestContext.Progress.WriteLine("Initializing WebDriver...");

            if (driver == null)
            {
                //driver = new FirefoxDriver();
                driver = new ChromeDriver();
            }

            _scenarioContext["WebDriver"] = driver;
            _scenario = _feature.CreateNode(_scenarioContext.ScenarioInfo.Title);
        }

        [AfterStep]
        public void InsertReportingSteps()
        {
            string stepText = _scenarioContext.StepContext.StepInfo.Text;
            string screenshotPath = CaptureScreenshot(_scenarioContext.ScenarioInfo.Title, stepText);

            if (_scenarioContext.TestError == null)
            {
                if (screenshotPath != null)
                {
                    _scenario.Log(Status.Pass, stepText,
                        MediaEntityBuilder.CreateScreenCaptureFromPath(screenshotPath).Build());
                }
                else
                {
                    _scenario.Log(Status.Pass, stepText);
                }
            }
            else
            {
                if (screenshotPath != null)
                {
                    _scenario.Log(Status.Fail, stepText,
                        MediaEntityBuilder.CreateScreenCaptureFromPath(screenshotPath).Build());
                }
                else
                {
                    _scenario.Log(Status.Fail, stepText);
                }

                _scenario.Log(Status.Fail, _scenarioContext.TestError.Message);
            }
        }

        [AfterScenario]
        public void TearDown()
        {
            if (driver != null)
            {
                driver.Quit();
                driver = null;
            }
        }

        [AfterTestRun]
        public static void AfterTestRun()
        {
            _extent.Flush();
            // Send email after the test run finishes
            SendEmailWithGmail();
        }

        private string CaptureScreenshot(string scenarioName, string stepName)
        {
            try
            {
                if (driver == null || driver.WindowHandles.Count == 0)
                {
                    TestContext.Progress.WriteLine("WebDriver is null or browser is closed. Skipping screenshot.");
                    return null;
                }

                Thread.Sleep(500);  // ✅ Small Wait Before Capturing Screenshot
                Screenshot screenshot = ((ITakesScreenshot)driver).GetScreenshot();

                string reportDirectory = Path.Combine(Directory.GetCurrentDirectory(), "Reports");
                string screenshotDirectory = Path.Combine(reportDirectory, "Screenshots");

                Directory.CreateDirectory(screenshotDirectory);  // ✅ Ensure Folder Exists

                // ✅ Generate a Safe Filename
                string sanitizedStepName = string.Join("_", stepName.Split(Path.GetInvalidFileNameChars()));
                string fileName = $"{scenarioName}_{sanitizedStepName}.png";
                string filePath = Path.Combine(screenshotDirectory, fileName);

                screenshot.SaveAsFile(filePath);
                TestContext.Progress.WriteLine($"Screenshot saved: {filePath}");

                return Path.Combine("Screenshots", fileName);  // ✅ Return Relative Path for Extent Report
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
                string senderEmail = "shankariu804@gmail.com"; // ✅ Replace with your Gmail address
                string senderPassword = "exry tjbv yrxb ctnu"; // ✅ Use the App Password (16 characters)
                string recipientEmail = "shankariu8@gmail.com"; // ✅ Replace with recipient's email
 
                MailMessage mail = new MailMessage
                {
                    From = new MailAddress(senderEmail),
                    Subject = "SpecFlow Test Report & Screenshots",
                    Body = "Attached are the Extent Report and failure screenshots from the latest test execution.",
                    IsBodyHtml = false
                };
 
                mail.To.Add(recipientEmail);
 
                // ✅ Attach Extent Report
                string reportPath = Path.Combine(Directory.GetCurrentDirectory(), "Reports", "ExtentReport.html");
                if (File.Exists(reportPath))
                    mail.Attachments.Add(new Attachment(reportPath));
 
                // ✅ Attach Screenshots (if any)
                string screenshotsDir = Path.Combine(Directory.GetCurrentDirectory(), "Reports", "Screenshots");
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
