using System;
using System.Collections.Generic;
using System.IO.IsolatedStorage;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PhoneApp16
{
    class SettingsManager
    {

        public string FASE_ATUAL = "faseAtual";
        public string PALAVRAS_FASE_ATUAL = "palavras";
        public string AUDIO_PALAVRAS = "audiopalavras";
        public string AUDIO_DICAS = "audiodicas";
        public string INDEX_FASE_ATUAL = "indexfaseatual";
        public string ACERTOS_FASE_ATUAL = "acertosFaseAtual";
        public string NOTA_FASE_1 = "notafase1";
        public string NOTA_FASE_2 = "notafase2";
        public string NOTA_FASE_3 = "notafase3";
        public string NOTA_FASE_4 = "notafase4";
        public string NOTA_FASE_5 = "notafase5";
        public string AVALIADO = "avaliadoloja";

        public void SetValue(string settingName, int value)
        {
            settingName = settingName.ToLower();
            if (IsolatedStorageSettings.ApplicationSettings.Contains(settingName))
            {
                IsolatedStorageSettings.ApplicationSettings[settingName] = value;
            }
            else
            {
                IsolatedStorageSettings.ApplicationSettings.Add(settingName, value);
                IsolatedStorageSettings.ApplicationSettings.Save();
            }

        }

        public int GetValue(string settingName, int defaultValue)
        {
            settingName = settingName.ToLower();
            if (IsolatedStorageSettings.ApplicationSettings.Contains(settingName))
            {
                return (int)IsolatedStorageSettings.ApplicationSettings[settingName];
            }
            else
            {
                return defaultValue;
            }
        }

        public void SetValue(string settingName, double value)
        {
            settingName = settingName.ToLower();
            if (IsolatedStorageSettings.ApplicationSettings.Contains(settingName))
            {
                IsolatedStorageSettings.ApplicationSettings[settingName] = value;
            }
            else
            {
                IsolatedStorageSettings.ApplicationSettings.Add(settingName, value);
                IsolatedStorageSettings.ApplicationSettings.Save();
            }

        }

        public double GetValue(string settingName, double defaultValue)
        {
            settingName = settingName.ToLower();
            if (IsolatedStorageSettings.ApplicationSettings.Contains(settingName))
            {
                return (double)IsolatedStorageSettings.ApplicationSettings[settingName];
            }
            else
            {
                return defaultValue;
            }
        }

        public void SetValue(string settingName, string[] value)
        {
            settingName = settingName.ToLower();
            if (IsolatedStorageSettings.ApplicationSettings.Contains(settingName))
            {
                IsolatedStorageSettings.ApplicationSettings[settingName] = value;
            }
            else
            {
                IsolatedStorageSettings.ApplicationSettings.Add(settingName, value);
                IsolatedStorageSettings.ApplicationSettings.Save();
            }

        }

        public string[] GetValue(string settingName, string[] defaultValue)
        {
            settingName = settingName.ToLower();
            if (IsolatedStorageSettings.ApplicationSettings.Contains(settingName))
            {
                return (string[])IsolatedStorageSettings.ApplicationSettings[settingName];
            }
            else
            {
                return defaultValue;
            }
        }

    }
}
