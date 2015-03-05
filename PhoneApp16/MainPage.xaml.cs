using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;
using PhoneApp16.Resources;
using Microsoft.Xna.Framework.Media;
using Microsoft.Xna.Framework;
using Microsoft.Phone.Tasks;
using Windows.ApplicationModel.Store;
using Windows.ApplicationModel;
using System.Windows.Threading;
using Microsoft.Devices;

namespace PhoneApp16
{
    public partial class MainPage : PhoneApplicationPage
    {

        SettingsManager SM;

        DispatcherTimer ConfirmarTimer;
        DispatcherTimer FimDoNivelTimer;
        DispatcherTimer TempoPalavra;

        VibrateController vibrateController;

        int tempoRestante;

        bool aprovado;

        bool waiting;
        bool palavraOuvida;

        string[] palavrasFaceis = 
            new string[] {"certo", "certeza", "churrascaria", "colégio", "solto", "conquista", "periquito", "conosco", "crise", "pesado",
                          "detalhe", "combustível", "deixar", "ensino", "estrela", "abacaxi", "formigueiro", "alimentação", "gelo", "hóspede",
                          "hotel", "revista", "jipe", "máximo", "mudança", "beijo", "pardal", "posse", "aliás", "puxão",
                          "bibelô", "judô", "rabanete", "aquário", "astro", "autor", "avisar", "azul", "anotação", "banheiro",
                          "sapato", "segredo", "sítio", "frase", "aditivo", "orelhas", "computador", "lençol", "sólido", "cebola"};
        int[] arquivosPalavrasFaceis = new int[] {100, 104, 112, 118, 123, 129, 132, 135, 140, 147, 
                                                  152, 15,  160, 175, 187, 1,   200, 20,  212, 223, 
                                                  224, 235, 248, 260, 273, 27,  290, 303, 30,  311,
                                                  322, 334, 347, 34,  40,  44,  48,  51,  56,  66,
                                                  70,  72,  75,  77,  7,   80,  83,  88,  92,  94};

        string[] palavrasMedias =
            new string[] {"traquéia", "chuchu", "agilizar", "compromisso", "jejum", "higiênico", "crescimento", "definição", "pechincha", "entrelaçado",
                          "estrangeiro", "fortíssimo", "gigantesco", "hortência", "informação", "zunzunzum", "jiló", "magnífico", "nebuloso", "particípio",
                          "paupérrimo", "pisca-pisca", "proliferar", "ambulância", "realeza", "ratazana", "biodiesel", "lombalgia", "reagir", "tênue",
                          "uirapuru", "utilização", "veterinário", "arranhão", "aconselhar", "antialérgico", "bem-sucedido", "ensanguentado", "tecnologia", "interestadual",
                          "plebeia", "reincidente", "superaquecido", "reforço", "preguiça", "celulose", "aquilo", "chá-mate", "gasoso", "crendice"};
        int[] arquivosPalavrasMedias = new int[] {100, 112,  11, 124, 136, 148, 154, 160,  16, 172, 
                                                  184, 200, 209, 222, 235, 240, 248, 259, 271, 282, 
                                                  287, 294, 306,  30, 317, 331, 342, 354, 366, 378,
                                                  383, 389, 394,  41,   4, 503, 516, 528,  53, 540,
                                                  552, 563, 576,  65,  71,  76,  80,  90,  94, 151};

        string[] palavrasDificeis =
            new string[] {"fictício", "imprescindível", "insolúvel", "xifópagos", "escarcéu", "joanopolense", "lisonja", "marmóreo", "percalço", "piscicultura",
                          "presidência", "recenseamento", "núpcias", "esterçar", "necrosar", "síntese", "universitário", "vitivinicultor", "zooterapia", "aerossol",
                          "benquisto", "carapuça", "circense", "coalhada", "arrulhar", "cordisburguense", "desengonçado", "eletrocussão", "endoidecer", "escaramuça",
                          "estrear", "exonerável", "fraseologia", "fúcsia", "popularização", "gipsografia", "hipótese", "adaptação", "interagir", "reconciliação",
                          "chapéu-de-sol", "disfarçar", "esquistossomose", "coordenador", "exaltação", "alto-falantes", "eclipse", "estripulia", "armísticio", "expressão"};
        int[] arquivosPalavrasDificeis = new int[] {101, 107, 113, 120, 125, 130, 134, 137, 142, 146, 
                                                    150, 156, 164,  16, 170, 176, 183, 189, 195, 200, 
                                                    207, 213, 220, 223,  22, 231, 241, 248, 255, 260,
                                                    267, 272, 280, 284,  28, 290, 297,   2,  33,  40,
                                                     47,  53,  60,  64,  70,  76,  83,  89,   8,  96};

        string palavraDigitada;
        bool acentoAgudoON;
        bool acentoCircunflexoON;
        bool acentoTioON;

        int faseAtual = 0; // sao 5 fases
        int acertosFaseAtual = 0;
        int indexPalavraFaseAtual = 0;
        string[] palavras;
        string[] audioPalavras;
        string[] audioDicas;
        int totalPalavrasFase = 20;


        double notaFase1 = 0;
        double notaFase2 = 0;
        double notaFase3 = 0;
        double notaFase4 = 0;
        double notaFase5 = 0;

        bool isTrial;

        // Constructor
        public MainPage()
        {
            InitializeComponent();

            //if (CurrentApp.LicenseInformation.IsTrial)
                isTrial = true;

            SM = new SettingsManager();

            carregaHistorico();

            faseAtual = SM.GetValue(SM.FASE_ATUAL, 0);

            if (faseAtual <= 0)
            {
                StartNewGame(1);
            }
            else
            {
                CarregaFaseAtual();
            }

            vibrateController = VibrateController.Default;

        }

        protected async override void OnBackKeyPress(System.ComponentModel.CancelEventArgs e)
        {
            if (SM.GetValue(SM.AVALIADO, 0) == 0)
            {
                var result = MessageBox.Show("Deseja avaliar o aplicativo na loja?", "Soletrando", MessageBoxButton.OKCancel);
                if (result == MessageBoxResult.OK)
                {
                    SM.SetValue(SM.AVALIADO, 1);
                    MarketplaceReviewTask review = new MarketplaceReviewTask();
                    review.Show();
                }
            }


            /*
            if (CurrentApp.LicenseInformation.IsTrial)
            {
                var result = MessageBox.Show("Essa é a versão de avaliação! Pressione OK para comprar a versão completa do aplicativo e ter todos os níveis desbloqueados. Comprando a versão completa você terá 150 palavras em 5 diferentes níveis: Ensino Fundamental, Ensino Médio, Graduação, Mestrado e Doutorado! DIVIRTA-SE!", "Soletrando", MessageBoxButton.OKCancel);
                if (result == MessageBoxResult.OK)
                {
                    string x = await CurrentApp.RequestAppPurchaseAsync(false);
                }
            }
             * */
            base.OnBackKeyPress(e);
        }


        private void carregaHistorico()
        {
            notaFase1 = SM.GetValue(SM.NOTA_FASE_1, -1.0);
            notaFase2 = SM.GetValue(SM.NOTA_FASE_2, -1.0);
            notaFase3 = SM.GetValue(SM.NOTA_FASE_3, -1.0);
            notaFase4 = SM.GetValue(SM.NOTA_FASE_4, -1.0);
            notaFase5 = SM.GetValue(SM.NOTA_FASE_5, -1.0);

            if (notaFase1 >=0 )
            {
                tbNotaFundamental.Text = "" + notaFase1;
                if (notaFase1 < 7)
                {
                    tbNotaFundamental.Foreground = tbNotaVermelha.Foreground;
                }
                else
                {
                    tbNotaFundamental.Foreground = tbNotaAzul.Foreground;
                }
            }
                
            if (notaFase2 >= 0)
            {
                tbNotaMedio.Text = "" + notaFase2;
                if (notaFase2 < 7)
                {
                    tbNotaMedio.Foreground = tbNotaVermelha.Foreground;
                }
                else
                {
                    tbNotaMedio.Foreground = tbNotaAzul.Foreground;
                }
            }
                

            if (notaFase3 >= 0)
            {
                tbNotaGraduacao.Text = "" + notaFase3;
                if (notaFase3 < 7)
                {
                    tbNotaGraduacao.Foreground = tbNotaVermelha.Foreground;
                }
                else
                {
                    tbNotaGraduacao.Foreground = tbNotaAzul.Foreground;
                }
            }
                

            if (notaFase4 >= 0)
            {
                tbNotaMestrado.Text = "" + notaFase4;
                if (notaFase4 < 7)
                {
                    tbNotaMestrado.Foreground = tbNotaVermelha.Foreground;
                }
                else
                {
                    tbNotaMestrado.Foreground = tbNotaAzul.Foreground;
                }
            }
                

            if (notaFase5 >= 0)
            {
                tbNotaDoutorado.Text = "" + notaFase5;
                if (notaFase5 < 7)
                {
                    tbNotaDoutorado.Foreground = tbNotaVermelha.Foreground;
                }
                else
                {
                    tbNotaDoutorado.Foreground = tbNotaAzul.Foreground;
                }
            }
                

        }

        private void CarregaFaseAtual()
        {
            faseAtual = SM.GetValue(SM.FASE_ATUAL, -1);
            if (faseAtual <= 0)
            {
                StartNewGame(1);
            }
            else
            {
                acertosFaseAtual = SM.GetValue(SM.ACERTOS_FASE_ATUAL, 0);
                indexPalavraFaseAtual = SM.GetValue(SM.INDEX_FASE_ATUAL,0);
                palavras = SM.GetValue(SM.PALAVRAS_FASE_ATUAL, null);
                audioPalavras = SM.GetValue(SM.AUDIO_PALAVRAS, null);
                audioDicas = SM.GetValue(SM.AUDIO_DICAS, null);
                if (palavras == null || audioDicas == null || audioPalavras == null)
                {
                    SorteiaPalavras();
                }

            }

            atualizaTextosTela();
        }


        private void StartNewGame(int fase)
        {
            faseAtual = fase;
            indexPalavraFaseAtual = 0; 
            acertosFaseAtual = 0;
            SM.SetValue(SM.FASE_ATUAL, faseAtual);
            SM.SetValue(SM.INDEX_FASE_ATUAL, indexPalavraFaseAtual);
            SM.SetValue(SM.ACERTOS_FASE_ATUAL, acertosFaseAtual);
            SorteiaPalavras();
            atualizaTextosTela();
        }

        private void SorteiaPalavras()
        {
            Random random = new Random();
            palavras = new string[totalPalavrasFase];
            audioPalavras = new string[totalPalavrasFase];
            audioDicas = new string[totalPalavrasFase];

            if (faseAtual == 1)
            {
                //20 palavras faceis
                for (int i = 0; i < totalPalavrasFase; i++)
                {
                    int palavraIndex = -1;
                    do{
                        palavraIndex = random.Next(50);
                        for (int j = 0; j < totalPalavrasFase; j++) //verifica se a palavra já foi sorteada
                        {
                            if (palavras[j] != null && palavras[j].Equals(palavrasFaceis[palavraIndex]))
                            {
                                palavraIndex = -1;
                                break;
                            }
                        }
                    } while (palavraIndex == -1);

                    palavras[i] = palavrasFaceis[palavraIndex];
                    audioPalavras[i] = arquivosPalavrasFaceis[palavraIndex] + "fp.mp3";
                    audioDicas[i] = arquivosPalavrasFaceis[palavraIndex] + "fd.mp3";
                }
            }
            else if (faseAtual == 2)
            {
                // 10 palavras faceis e 10 palavras medias

                //10 palavras faceis
                for (int i = 0; i < 10; i++)
                {
                    int palavraIndex = -1;
                    do
                    {
                        palavraIndex = random.Next(50);
                        for (int j = 0; j < 10; j++) //verifica se a palavra já foi sorteada
                        {
                            if (palavras[j] != null && palavras[j].Equals(palavrasFaceis[palavraIndex]))
                            {
                                palavraIndex = -1;
                                break;
                            }
                        }
                    } while (palavraIndex == -1);

                    palavras[i] = palavrasFaceis[palavraIndex];
                    audioPalavras[i] = arquivosPalavrasFaceis[palavraIndex] + "fp.mp3";
                    audioDicas[i] = arquivosPalavrasFaceis[palavraIndex] + "fd.mp3";
                }

                //10 palavras medias
                for (int i = 10; i < 20; i++)
                {
                    int palavraIndex = -1;
                    do
                    {
                        palavraIndex = random.Next(50);
                        for (int j = 10; j < 20; j++) //verifica se a palavra já foi sorteada
                        {
                            if (palavras[j] != null && palavras[j].Equals(palavrasMedias[palavraIndex]))
                            {
                                palavraIndex = -1;
                                break;
                            }
                        }
                    } while (palavraIndex == -1);

                    palavras[i] = palavrasMedias[palavraIndex];
                    audioPalavras[i] = arquivosPalavrasMedias[palavraIndex] + "mp.mp3";
                    audioDicas[i] = arquivosPalavrasMedias[palavraIndex] + "md.mp3";
                }
            }
            else if (faseAtual == 3)
            {
                // 5 palavras faceis, 10 palavras medias e 5 palavras dificeis

                //5 palavras faceis
                for (int i = 0; i < 5; i++)
                {
                    int palavraIndex = -1;
                    do
                    {
                        palavraIndex = random.Next(50);
                        for (int j = 0; j < 5; j++) //verifica se a palavra já foi sorteada
                        {
                            if (palavras[j] != null && palavras[j].Equals(palavrasFaceis[palavraIndex]))
                            {
                                palavraIndex = -1;
                                break;
                            }
                        }
                    } while (palavraIndex == -1);

                    palavras[i] = palavrasFaceis[palavraIndex];
                    audioPalavras[i] = arquivosPalavrasFaceis[palavraIndex] + "fp.mp3";
                    audioDicas[i] = arquivosPalavrasFaceis[palavraIndex] + "fd.mp3";
                }

                //10 palavras medias
                for (int i = 5; i < 15; i++)
                {
                    int palavraIndex = -1;
                    do
                    {
                        palavraIndex = random.Next(50);
                        for (int j = 5; j < 15; j++) //verifica se a palavra já foi sorteada
                        {
                            if (palavras[j] != null && palavras[j].Equals(palavrasMedias[palavraIndex]))
                            {
                                palavraIndex = -1;
                                break;
                            }
                        }
                    } while (palavraIndex == -1);

                    palavras[i] = palavrasMedias[palavraIndex];
                    audioPalavras[i] = arquivosPalavrasMedias[palavraIndex] + "mp.mp3";
                    audioDicas[i] = arquivosPalavrasMedias[palavraIndex] + "md.mp3";
                }

                //5 palavras dificeis
                for (int i = 15; i < 20; i++)
                {
                    int palavraIndex = -1;
                    do
                    {
                        palavraIndex = random.Next(50);
                        for (int j = 15; j < 20; j++) //verifica se a palavra já foi sorteada
                        {
                            if (palavras[j] != null && palavras[j].Equals(palavrasDificeis[palavraIndex]))
                            {
                                palavraIndex = -1;
                                break;
                            }
                        }
                    } while (palavraIndex == -1);

                    palavras[i] = palavrasDificeis[palavraIndex];
                    audioPalavras[i] = arquivosPalavrasDificeis[palavraIndex] + "dp.mp3";
                    audioDicas[i] = arquivosPalavrasDificeis[palavraIndex] + "dd.mp3";
                }
            }

            else if (faseAtual == 4)
            {
                // 10 palavras medias e 10 palavras dificeis

                //10 palavras medias
                for (int i = 0; i < 10; i++)
                {
                    int palavraIndex = -1;
                    do
                    {
                        palavraIndex = random.Next(50);
                        for (int j = 0; j < 10; j++) //verifica se a palavra já foi sorteada
                        {
                            if (palavras[j] != null && palavras[j].Equals(palavrasMedias[palavraIndex]))
                            {
                                palavraIndex = -1;
                                break;
                            }
                        }
                    } while (palavraIndex == -1);

                    palavras[i] = palavrasMedias[palavraIndex];
                    audioPalavras[i] = arquivosPalavrasMedias[palavraIndex] + "mp.mp3";
                    audioDicas[i] = arquivosPalavrasMedias[palavraIndex] + "md.mp3";
                }

                //10 palavras dificeis
                for (int i = 10; i < 20; i++)
                {
                    int palavraIndex = -1;
                    do
                    {
                        palavraIndex = random.Next(50);
                        for (int j = 10; j < 20; j++) //verifica se a palavra já foi sorteada
                        {
                            if (palavras[j] != null && palavras[j].Equals(palavrasDificeis[palavraIndex]))
                            {
                                palavraIndex = -1;
                                break;
                            }
                        }
                    } while (palavraIndex == -1);

                    palavras[i] = palavrasDificeis[palavraIndex];
                    audioPalavras[i] = arquivosPalavrasDificeis[palavraIndex] + "dp.mp3";
                    audioDicas[i] = arquivosPalavrasDificeis[palavraIndex] + "dd.mp3";
                }
            }
            else if (faseAtual == 5)
            {
                // 20 palavras dificeis

                //20 palavras dificeis
                for (int i = 0; i < 20; i++)
                {
                    int palavraIndex = -1;
                    do
                    {
                        palavraIndex = random.Next(50);
                        for (int j = 0; j < 20; j++) //verifica se a palavra já foi sorteada
                        {
                            if (palavras[j] != null && palavras[j].Equals(palavrasDificeis[palavraIndex]))
                            {
                                palavraIndex = -1;
                                break;
                            }
                        }
                    } while (palavraIndex == -1);

                    palavras[i] = palavrasDificeis[palavraIndex];
                    audioPalavras[i] = arquivosPalavrasDificeis[palavraIndex] + "dp.mp3";
                    audioDicas[i] = arquivosPalavrasDificeis[palavraIndex] + "dd.mp3";
                }
            }

            SM.SetValue(SM.PALAVRAS_FASE_ATUAL, palavras);
            SM.SetValue(SM.AUDIO_PALAVRAS, audioPalavras);
            SM.SetValue(SM.AUDIO_DICAS, audioDicas);
        }

        private void Image_Tap(object sender, System.Windows.Input.GestureEventArgs e)
        {
            if (!waiting && palavraOuvida)
            {
                
                vibrateController.Start(TimeSpan.FromMilliseconds(20));

                if (sender == imgA)
                {
                    if (acentoAgudoON) palavraDigitada += "Á";
                    else if (acentoCircunflexoON) palavraDigitada += "Â";
                    else if (acentoTioON) palavraDigitada += "Ã";
                    else palavraDigitada += "A";
                }
                else if (sender == imgE)
                {
                    if (acentoAgudoON) palavraDigitada += "É";
                    else if (acentoCircunflexoON) palavraDigitada += "Ê";
                    else palavraDigitada += "E";
                }
                else if (sender == imgI)
                {
                    if (acentoAgudoON) palavraDigitada += "Í";
                    else palavraDigitada += "I";
                }
                else if (sender == imgO)
                {
                    if (acentoAgudoON) palavraDigitada += "Ó";
                    else if (acentoCircunflexoON) palavraDigitada += "Ô";
                    else if (acentoTioON) palavraDigitada += "Õ";
                    else palavraDigitada += "O";
                }
                else if (sender == imgU)
                {
                    if (acentoAgudoON) palavraDigitada += "Ú";
                    else palavraDigitada += "U";
                }

                acentoAgudoON = false;
                acentoCircunflexoON = false;
                acentoTioON = false;

                if (sender == imgB) palavraDigitada += "B";
                else if (sender == imgC) palavraDigitada += "C";
                else if (sender == imgCcedilha) palavraDigitada += "Ç";
                else if (sender == imgD) palavraDigitada += "D";

                else if (sender == imgF) palavraDigitada += "F";
                else if (sender == imgG) palavraDigitada += "G";
                else if (sender == imgH) palavraDigitada += "H";

                else if (sender == imgJ) palavraDigitada += "J";
                else if (sender == imgK) palavraDigitada += "K";
                else if (sender == imgL) palavraDigitada += "L";
                else if (sender == imgM) palavraDigitada += "M";
                else if (sender == imgN) palavraDigitada += "N";

                else if (sender == imgP) palavraDigitada += "P";
                else if (sender == imgQ) palavraDigitada += "Q";
                else if (sender == imgR) palavraDigitada += "R";
                else if (sender == imgS) palavraDigitada += "S";
                else if (sender == imgT) palavraDigitada += "T";

                else if (sender == imgV) palavraDigitada += "V";
                else if (sender == imgW) palavraDigitada += "W";
                else if (sender == imgX) palavraDigitada += "X";
                else if (sender == imgY) palavraDigitada += "Y";
                else if (sender == imgZ) palavraDigitada += "Z";
                else if (sender == imgHifen) palavraDigitada += "-";
                else if (sender == imgAcentoAgudo) acentoAgudoON = true;
                else if (sender == imgAcentoCircunflexo) acentoCircunflexoON = true;
                else if (sender == imgAcentoTio) acentoTioON = true;
                else if (sender == imgSeta)
                {
                    palavraDigitada = palavraDigitada.Substring(0, palavraDigitada.Length - 1);
                }


                tbPalavraDigitada.Text = palavraDigitada;
            }

        }


        private void imgPalavra_Tap(object sender, System.Windows.Input.GestureEventArgs e)
        {
            if (!waiting)
            {
                vibrateController.Start(TimeSpan.FromMilliseconds(20));

                player.Source = new Uri("/Assets/audio/" + audioPalavras[indexPalavraFaseAtual], UriKind.Relative);
                player.Stop();
                player.Play();
                palavraOuvida = true;
                if ((TempoPalavra == null)||(TempoPalavra != null && !TempoPalavra.IsEnabled))
                {
                    tempoRestante = 45;
                    TempoPalavra = new DispatcherTimer();
                    TempoPalavra.Tick += TempoPalavra_Tick;
                    TempoPalavra.Interval = new TimeSpan(0, 0, 1);
                    TempoPalavra.Start();
                }
            }
        }

        void TempoPalavra_Tick(object sender, EventArgs e)
        {
            tempoRestante--;
            tbTempo.Text = "" + tempoRestante;
            if (tempoRestante == 8)
            {
                playTimeEnding();
            }
            if (tempoRestante <= 0)
            {
                imgConfirmar_Tap(null, null);
                TempoPalavra.Stop();
            }
        }

        private void imgDica_Tap(object sender, System.Windows.Input.GestureEventArgs e)
        {
            if (!waiting && palavraOuvida)
            {
                vibrateController.Start(TimeSpan.FromMilliseconds(20));
                player.Source = new Uri("/Assets/audio/" + audioDicas[indexPalavraFaseAtual], UriKind.Relative);
                player.Stop();
                player.Play();
            }
        }

        private void playRightAnswer()
        {
            player.Source = new Uri("/Assets/audio/right.mp3", UriKind.Relative);
            player.Stop();
            player.Play();
        }

        private void playWrongAnswer()
        {
            player.Source = new Uri("/Assets/audio/wrong.mp3", UriKind.Relative);
            player.Stop();
            player.Play();
        }

        private void playTimeEnding()
        {
            player.Source = new Uri("/Assets/audio/tick.mp3", UriKind.Relative);
            player.Stop();
            player.Play();
        }

        private void imgConfirmar_Tap(object sender, System.Windows.Input.GestureEventArgs e)
        {
            if (!waiting && palavraOuvida)
            {
                vibrateController.Start(TimeSpan.FromMilliseconds(20));
                TempoPalavra.Stop();
                palavraOuvida = false;
                waiting = true;
                if (palavraDigitada == null)
                    palavraDigitada = "";
                if (palavraDigitada.ToLower().Equals(palavras[indexPalavraFaseAtual]))
                {
                    playRightAnswer();
                    imgCerto.Visibility = System.Windows.Visibility.Visible;
                    acertosFaseAtual++;
                }
                else
                {
                    playWrongAnswer();
                    imgErrado.Visibility = System.Windows.Visibility.Visible;
                    tbPalavraCorrigida.Text = palavras[indexPalavraFaseAtual].ToUpper();
                    tbPalavraCorrigida.Visibility = System.Windows.Visibility.Visible;
                }
                atualizaTextosTela();

                SM.SetValue(SM.ACERTOS_FASE_ATUAL, acertosFaseAtual);
                indexPalavraFaseAtual++;
                SM.SetValue(SM.INDEX_FASE_ATUAL, indexPalavraFaseAtual);

                ConfirmarTimer = new DispatcherTimer();
                ConfirmarTimer.Tick += ConfirmarTimer_Tick;
                ConfirmarTimer.Interval = new TimeSpan(0, 0, 3);
                ConfirmarTimer.Start();
            }
        }

        void ConfirmarTimer_Tick(object sender, EventArgs e)
        {
            ConfirmarTimer.Stop();
            if (indexPalavraFaseAtual < totalPalavrasFase)
            {
                palavraDigitada = "";
                tbTempo.Text = "45";
                tbPalavraDigitada.Text = palavraDigitada;
                tbPalavraCorrigida.Visibility = System.Windows.Visibility.Collapsed;
                tbPalavraCorrigida.Text = "";
                imgCerto.Visibility = System.Windows.Visibility.Collapsed;
                imgErrado.Visibility = System.Windows.Visibility.Collapsed;
                waiting = false;
            }
            else
            {
                imgCerto.Visibility = System.Windows.Visibility.Collapsed;
                imgErrado.Visibility = System.Windows.Visibility.Collapsed;     
                FimDoNivel();    
            }

            atualizaTextosTela();
            
        }

        private void FimDoNivel()
        {
            if (faseAtual == 1)
            {
                notaFase1 = ((double)(acertosFaseAtual / 2.0));
                palavraDigitada = "NOTA FINAL: " + notaFase1;
                tbPalavraDigitada.Text = palavraDigitada;
                SM.SetValue(SM.NOTA_FASE_1, notaFase1);
                tbNotaFundamental.Text = "" + notaFase1;
                if (notaFase1 < 7)
                {
                    tbNotaFundamental.Foreground = tbNotaVermelha.Foreground;
                    tbPalavraCorrigida.Text = "Sua nota deve ser maior que 7 para passar para o próximo nível.";
                    tbPalavraCorrigida.FontSize = 30;
                    tbPalavraCorrigida.Visibility = System.Windows.Visibility.Visible;
                    tbPalavraCorrigida.Foreground = tbNotaVermelha.Foreground;
                    aprovado = false;
                }
                else
                {
                    tbNotaFundamental.Foreground = tbNotaAzul.Foreground;
                    tbPalavraCorrigida.Text = "Parabéns! Você foi aprovado para o próximo nível!";
                    tbPalavraCorrigida.FontSize = 30;
                    tbPalavraCorrigida.Foreground = tbNotaAzul.Foreground;
                    tbPalavraCorrigida.Visibility = System.Windows.Visibility.Visible;
                    aprovado = true;
                }
            }
            else if (faseAtual == 2)
            {
                notaFase2 = ((double)(acertosFaseAtual / 2.0));
                palavraDigitada = "NOTA FINAL: " + notaFase2;
                tbPalavraDigitada.Text = palavraDigitada;
                SM.SetValue(SM.NOTA_FASE_2, notaFase2);
                tbNotaMedio.Text = "" + notaFase2;
                if (notaFase2 < 7)
                {
                    tbNotaMedio.Foreground = tbNotaVermelha.Foreground;
                    tbPalavraCorrigida.Text = "Sua nota deve ser maior que 7 para passar para o próximo nível.";
                    tbPalavraCorrigida.FontSize = 30;
                    tbPalavraCorrigida.Visibility = System.Windows.Visibility.Visible;
                    tbPalavraCorrigida.Foreground = tbNotaVermelha.Foreground;
                    aprovado = false;
                }
                else
                {
                    tbNotaMedio.Foreground = tbNotaAzul.Foreground;
                    tbPalavraCorrigida.Text = "Parabéns! Você foi aprovado para o próximo nível!";
                    tbPalavraCorrigida.FontSize = 30;
                    tbPalavraCorrigida.Foreground = tbNotaAzul.Foreground;
                    tbPalavraCorrigida.Visibility = System.Windows.Visibility.Visible;
                    aprovado = true;
                }
            }
            else if (faseAtual == 3)
            {
                notaFase3 = ((double)(acertosFaseAtual / 2.0));
                palavraDigitada = "NOTA FINAL: " + notaFase3;
                tbPalavraDigitada.Text = palavraDigitada;
                SM.SetValue(SM.NOTA_FASE_3, notaFase3);
                tbNotaGraduacao.Text = "" + notaFase3;
                if (notaFase3 < 7)
                {
                    tbNotaGraduacao.Foreground = tbNotaVermelha.Foreground;
                    tbPalavraCorrigida.Text = "Sua nota deve ser maior que 7 para passar para o próximo nível.";
                    tbPalavraCorrigida.FontSize = 30;
                    tbPalavraCorrigida.Visibility = System.Windows.Visibility.Visible;
                    tbPalavraCorrigida.Foreground = tbNotaVermelha.Foreground;
                    aprovado = false;
                }
                else
                {
                    tbNotaGraduacao.Foreground = tbNotaAzul.Foreground;
                    tbPalavraCorrigida.Text = "Parabéns! Você foi aprovado para o próximo nível!";
                    tbPalavraCorrigida.FontSize = 30;
                    tbPalavraCorrigida.Foreground = tbNotaAzul.Foreground;
                    tbPalavraCorrigida.Visibility = System.Windows.Visibility.Visible;
                    aprovado = true;
                }
            }
            else if (faseAtual == 4)
            {
                notaFase4 = ((double)(acertosFaseAtual / 2.0));
                palavraDigitada = "NOTA FINAL: " + notaFase4;
                tbPalavraDigitada.Text = palavraDigitada;
                SM.SetValue(SM.NOTA_FASE_4, notaFase4);
                tbNotaMestrado.Text = "" + notaFase4;
                if (notaFase4 < 7)
                {
                    tbNotaMestrado.Foreground = tbNotaVermelha.Foreground;
                    tbPalavraCorrigida.Text = "Sua nota deve ser maior que 7 para passar para o próximo nível.";
                    tbPalavraCorrigida.FontSize = 30;
                    tbPalavraCorrigida.Visibility = System.Windows.Visibility.Visible;
                    tbPalavraCorrigida.Foreground = tbNotaVermelha.Foreground;
                    aprovado = false;
                }
                else
                {
                    tbNotaMestrado.Foreground = tbNotaAzul.Foreground;
                    tbPalavraCorrigida.Text = "Parabéns! Você foi aprovado para o próximo nível!";
                    tbPalavraCorrigida.FontSize = 30;
                    tbPalavraCorrigida.Foreground = tbNotaAzul.Foreground;
                    tbPalavraCorrigida.Visibility = System.Windows.Visibility.Visible;
                    aprovado = true;
                }
            }
            else if (faseAtual == 5)
            {
                notaFase5 = ((double)(acertosFaseAtual / 2.0));
                palavraDigitada = "NOTA FINAL: " + notaFase5;
                tbPalavraDigitada.Text = palavraDigitada;
                SM.SetValue(SM.NOTA_FASE_5, notaFase5);
                tbNotaDoutorado.Text = "" + notaFase5;
                if (notaFase5 < 7)
                {
                    tbNotaDoutorado.Foreground = tbNotaVermelha.Foreground;
                    tbPalavraCorrigida.Text = "Sua nota deve ser maior que 7 para passar para o próximo nível.";
                    tbPalavraCorrigida.FontSize = 30;
                    tbPalavraCorrigida.Visibility = System.Windows.Visibility.Visible;
                    tbPalavraCorrigida.Foreground = tbNotaVermelha.Foreground;
                    aprovado = false;
                }
                else
                {
                    tbNotaDoutorado.Foreground = tbNotaAzul.Foreground;
                    tbPalavraCorrigida.Text = "Parabéns! Você foi aprovado em todos os níveis!";
                    tbPalavraCorrigida.FontSize = 30;
                    tbPalavraCorrigida.Foreground = tbNotaAzul.Foreground;
                    tbPalavraCorrigida.Visibility = System.Windows.Visibility.Visible;
                    aprovado = true;
                }
            }

            FimDoNivelTimer = new DispatcherTimer();
            FimDoNivelTimer.Tick += FimDoNivelTimer_Tick;
            FimDoNivelTimer.Interval = new TimeSpan(0, 0, 6);
            FimDoNivelTimer.Start();
        }

        async void FimDoNivelTimer_Tick(object sender, EventArgs e)
        {
            FimDoNivelTimer.Stop();
            tbPalavraCorrigida.Text = "";
            tbPalavraCorrigida.FontSize = 40;
            tbPalavraCorrigida.Visibility = System.Windows.Visibility.Collapsed;
            tbPalavraCorrigida.Foreground = tbNotaVermelha.Foreground;
            tbPalavraDigitada.Text = "";
            palavraDigitada = "";
            if (!aprovado)
            {
                StartNewGame(faseAtual);
            }
            else
            {
                if (CurrentApp.LicenseInformation.IsTrial)
                {
                    var result = MessageBox.Show("Essa é a versão de avaliação! Pressione OK para comprar a versão completa do aplicativo e ter todos os níveis desbloqueados. Comprando a versão completa você terá 150 palavras em 5 diferentes níveis: Ensino Fundamental, Ensino Médio, Graduação, Mestrado e Doutorado! DIVIRTA-SE!", "Soletrando", MessageBoxButton.OKCancel);
                    if (result == MessageBoxResult.OK)
                    {
                        string x = await CurrentApp.RequestAppPurchaseAsync(false);
                    }
                }
                else if (faseAtual < 5)
                    StartNewGame(faseAtual + 1);
                else
                    StartNewGame(faseAtual);
            }
            waiting = false;
        }

        private void atualizaTextosTela()
        {
            tbAcertos.Text = "" + acertosFaseAtual;
            tbTempo.Text = "45";
            tbTotalPalavras.Text = "" + (indexPalavraFaseAtual + 1) + "/" + totalPalavrasFase;
            if (faseAtual >= 1)
            {
                tbNivel.Text = "Nível: Ensino Fundamental";
            }
            if (faseAtual >= 2)
            {
                tbNivel.Text = "Nível: Ensino Médio";
                imgCanetaPB.Visibility = System.Windows.Visibility.Collapsed;
                imgCaneta.Visibility = System.Windows.Visibility.Visible;
            }
            if (faseAtual >= 3)
            {
                tbNivel.Text = "Nível: Graduação";
                imgDiplomaPB.Visibility = System.Windows.Visibility.Collapsed;
                imgDiploma.Visibility = System.Windows.Visibility.Visible;
            }
            if (faseAtual >= 4)
            {
                tbNivel.Text = "Nível: Mestrado";
                imgChapeuPB.Visibility = System.Windows.Visibility.Collapsed;
                imgChapeu.Visibility = System.Windows.Visibility.Visible;
            }
            if (faseAtual == 5)
            {
                tbNivel.Text = "Nível: Doutorado";
                imgPHDPB.Visibility = System.Windows.Visibility.Collapsed;
                imgPHD.Visibility = System.Windows.Visibility.Visible;
            }
                
        }

        private void imgLapis_Tap(object sender, System.Windows.Input.GestureEventArgs e)
        {
            if (!waiting)
            {
                vibrateController.Start(TimeSpan.FromMilliseconds(20));
                var result = MessageBox.Show("Deseja jogar o nível Ensino Fundamental?", "Soletrando", MessageBoxButton.OKCancel);
                if (result == MessageBoxResult.OK)
                {
                    if (TempoPalavra != null)
                        TempoPalavra.Stop();
                    StartNewGame(1);
                }
            }
        }

        private void imgCaneta_Tap(object sender, System.Windows.Input.GestureEventArgs e)
        {
            if (!waiting)
            {
                vibrateController.Start(TimeSpan.FromMilliseconds(20));
                var result = MessageBox.Show("Deseja jogar o nível Ensino Médio?", "Soletrando", MessageBoxButton.OKCancel);
                if (result == MessageBoxResult.OK)
                {
                    if (TempoPalavra != null)
                        TempoPalavra.Stop();
                    StartNewGame(2);
                }
            }
        }

        private void imgCanetaPB_Tap(object sender, System.Windows.Input.GestureEventArgs e)
        {
            vibrateController.Start(TimeSpan.FromMilliseconds(20));
            MessageBox.Show("Nível Ensino Médio ainda não disponível. Você deve passar pelo nível anterior para poder jogar esse nível", "Soletrando", MessageBoxButton.OK);
        }

        private void imgDiploma_Tap(object sender, System.Windows.Input.GestureEventArgs e)
        {
            if (!waiting)
            {
                vibrateController.Start(TimeSpan.FromMilliseconds(20));
                var result = MessageBox.Show("Deseja jogar o nível Graduação?", "Soletrando", MessageBoxButton.OKCancel);
                if (result == MessageBoxResult.OK)
                {
                    if (TempoPalavra != null)
                        TempoPalavra.Stop();
                    StartNewGame(3);
                }
            }
        }

        private void imgDiplomaPB_Tap(object sender, System.Windows.Input.GestureEventArgs e)
        {
            vibrateController.Start(TimeSpan.FromMilliseconds(20));
            MessageBox.Show("Nível Graduação ainda não disponível. Você deve passar pelo nível anterior para poder jogar esse nível", "Soletrando", MessageBoxButton.OK);
        }

        private void imgChapeu_Tap(object sender, System.Windows.Input.GestureEventArgs e)
        {
            if (!waiting)
            {
                vibrateController.Start(TimeSpan.FromMilliseconds(20));
                var result = MessageBox.Show("Deseja jogar o nível Mestrado?", "Soletrando", MessageBoxButton.OKCancel);
                if (result == MessageBoxResult.OK)
                {
                    if (TempoPalavra != null)
                        TempoPalavra.Stop();
                    StartNewGame(4);
                }
            }
        }

        private void imgChapeuPB_Tap(object sender, System.Windows.Input.GestureEventArgs e)
        {
            vibrateController.Start(TimeSpan.FromMilliseconds(20));
            MessageBox.Show("Nível Mestrado ainda não disponível. Você deve passar pelo nível anterior para poder jogar esse nível", "Soletrando", MessageBoxButton.OK);
        }

        private void imgPHD_Tap(object sender, System.Windows.Input.GestureEventArgs e)
        {
            if (!waiting)
            {
                vibrateController.Start(TimeSpan.FromMilliseconds(20));
                var result = MessageBox.Show("Deseja jogar o nível Doutorado?", "Soletrando", MessageBoxButton.OKCancel);
                if (result == MessageBoxResult.OK)
                {
                    if (TempoPalavra != null)
                        TempoPalavra.Stop();
                    StartNewGame(5);
                }
            }
        }

        private void imgPHDPB_Tap(object sender, System.Windows.Input.GestureEventArgs e)
        {
            vibrateController.Start(TimeSpan.FromMilliseconds(20));
            MessageBox.Show("Nível Doutorado ainda não disponível. Você deve passar pelo nível anterior para poder jogar esse nível", "Soletrando", MessageBoxButton.OK);
        }

        private void Image_Tap_1(object sender, System.Windows.Input.GestureEventArgs e)
        {
            MarketplaceReviewTask review = new MarketplaceReviewTask();
            review.Show();
        }

    }
}