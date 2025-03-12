

using System;
using OpenQA.Selenium;
using SwagProject.Pages;
using TechTalk.SpecFlow;

namespace SwagProject.StepDefinitions
{
    [Binding]
    public class CheckoutStepDefinitions
    {
        private readonly Checkout check;

        public CheckoutStepDefinitions()
        {
            check = new Checkout();
        }

        [When(@"User clicks checkout button")]
        public void WhenUserClicksCheckoutButton()
        {
            check.Checkout();
            Thread.Sleep(1000);
        }

        [When(@"User enters ""([^""]*)"", ""([^""]*)"", and ""([^""]*)""")]
        public void WhenUserEntersFirstNameLastNameAndZipCode(string firstname, string lastname, string zipcode)
        {
            check.EnterDetails(firstname, lastname, zipcode);
            Thread.Sleep(1000);
        }

        [Then(@"Then Clicks on Continue")]
        public void ThenThenClicksOnContinue()
        {
            check.Continued();
            Thread.Sleep(1000);
        }
    }
}

