using TechTalk.SpecFlow;

namespace tikitwo_steam_cleaner.Test
{
    [Binding]
    public class CommonSteps
    {
        [Given(@"I have application settings")]
        public void GivenIHaveApplicationSettings(Table table)
        {
            ScenarioContext.Current.Pending();
        }

        [When(@"I load the application settings")]
        public void WhenILoadTheApplicationSettings()
        {
            ScenarioContext.Current.Pending();
        }

        [Then(@"I should have application settings")]
        public void ThenIShouldHaveApplicationSettings(Table table)
        {
            ScenarioContext.Current.Pending();
        }
    }
}