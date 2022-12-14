using CookBookApp.Data;
using CookBookApp.Model;
using CookBookApp.Model.Services;
using System.Globalization;
using System.Threading;
using System.Threading.Tasks;

namespace CookBookApp.Helpers
{
    public class UserSettingsManager
    {
        static UserSettings userProperties = new UserSettings();
        LanguageService languageService;
        public UserSettingsManager()
        {
            languageService= new LanguageService();
        }

        public string getUserName()
        {
            return userProperties.UserName;
        }

        public Language getLanguage()
        {
            int languageID = userProperties.LanguageID;
            Language languageResult = new Language();
            Task.Run(async () => {
                languageResult = await languageService.getLanguageByIDAsync(languageID);
            }).Wait();
            return languageResult;
        }

        public async Task<bool> setUserName(string newUserName)
        {
            userProperties.UserName = newUserName;
            return await Task.FromResult(true);
        }

        public async Task<bool> setUserLanguage(Language newLanguage)
        {
            userProperties.LanguageID = newLanguage.ID;
            setAppLanguage();
            return await Task.FromResult(true);
        }

        public void setAppLanguage()
        {
            string languageName = getLanguage().LanguageName;
            Thread.CurrentThread.CurrentUICulture = CultureInfo.GetCultureInfo(languageName);
        }
    }
}
