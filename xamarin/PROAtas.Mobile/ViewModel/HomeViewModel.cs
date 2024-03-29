﻿using Acr.UserDialogs;
using Craftz.Core;
using Craftz.ViewModel;
using Newtonsoft.Json;
using PROAtas.Core;
using PROAtas.Core.Model.Entities;
using PROAtas.Mobile.Services.Platform;
using PROAtas.Mobile.Services.Shared;
using PROAtas.Mobile.ViewModel.Elements;
using Syncfusion.DocIO.DLS;
using Syncfusion.DocIORenderer;
using Syncfusion.Pdf;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Xamarin.CommunityToolkit.ObjectModel;
using Xamarin.Forms;

namespace PROAtas.Mobile.ViewModel
{
    public class HomeViewModel : BaseViewModel
    {
        private readonly IAdService adService;
        private readonly IDataService dataService;
        private readonly IImageService imageService;
        private readonly IPrintService printService;
        private readonly IPermissionService permissionService;

        public HomeViewModel()
        {
            adService = DependencyService.Get<IAdService>();
            dataService = DependencyService.Get<IDataService>();
            imageService = DependencyService.Get<IImageService>();
            printService = DependencyService.Get<IPrintService>();
            permissionService = DependencyService.Get<IPermissionService>();
        }

        private bool isRewarded;

        #region Bindable Properties

        public ObservableRangeCollection<MinuteElement> Minutes { get; } = new ObservableRangeCollection<MinuteElement>();

        public bool IsSelectionEmpty => !HasMinuteSelected;
        public bool HasMinuteSelected => SelectedMinute != null;
        public MinuteElement SelectedMinute
        {
            get => _selectedMinute;
            set { _selectedMinute = value; OnPropertyChanged(); OnPropertyChanged(nameof(HasMinuteSelected)); OnPropertyChanged(nameof(IsSelectionEmpty)); }
        }
        MinuteElement _selectedMinute;

        #endregion

        #region Commands

        public Command SearchMinute
        {
            get { if (_searchMinute == null) _searchMinute = new Command(SearchMinuteExecute); return _searchMinute; }
        }
        private Command _searchMinute;
        private void SearchMinuteExecute()
        {

        }

        public Command<MinuteElement> SelectMinute
        {
            get { if (_selectMinute == null) _selectMinute = new Command<MinuteElement>(SelectMinuteExecute); return _selectMinute; }
        }
        private Command<MinuteElement> _selectMinute;
        private void SelectMinuteExecute(MinuteElement selectedMinute)
        {
            if (selectedMinute == SelectedMinute)
            {
                ClearSelection?.Execute(null);
            }
            else
            {
                // Unselect current minute
                if (SelectedMinute != null)
                    SelectedMinute.IsSelected = false;

                // Replace minute
                selectedMinute.IsSelected = true;
                SelectedMinute = selectedMinute;
            }
        }

        public Command CreateMinute
        {
            get { if (_createMinute == null) _createMinute = new Command(CreateMinuteExecute); return _createMinute; }
        }
        private Command _createMinute;
        private void CreateMinuteExecute()
        {
            logService.LogActionAsync(() => SetBusyAsync(async () =>
            {
                var organizationName = App.Current.Properties[AppConsts.OrganizationName]?.ToString() ?? "Nova Organização";

                var minute = new Minute();
                minute.Id = Guid.NewGuid().ToString();
                minute.Name = $"{organizationName} {DateTime.Today.ToString(Formats.DateFormat).Replace("/", "-")}";
                minute.Date = DateTime.Today.Date.ToString(Formats.DateFormat);
                minute.Start = DateTime.Now.TimeOfDay.ToString(Formats.TimeFormat);
                minute.End = DateTime.Now.TimeOfDay.ToString(Formats.TimeFormat);
                minute.Active = true;

                dataService.MinuteRepository.Add(minute);

                var topic = new Topic();
                topic.IdMinute = minute.Id;
                topic.Order = 1;
                topic.Text = "Tópico 1";

                dataService.TopicRepository.Add(topic);

                await Task.Delay(300);

                var jsonStr = JsonConvert.SerializeObject(minute);
                await Shell.Current.GoToAsync($"minute/?model={jsonStr}", true);
            }),
            log =>
            {
                if (log != null)
                    UserDialogs.Instance.Alert(log);

                return Task.CompletedTask;
            });
        }

        public Command EditMinute
        {
            get { if (_editMinute == null) _editMinute = new Command(EditMinuteExecute); return _editMinute; }
        }
        private Command _editMinute;
        private void EditMinuteExecute()
        {
            _ = SetBusyAsync(async () =>
            {
                await logService.LogActionAsync(async () =>
                {
                    await Task.Delay(300);

                    var jsonStr = JsonConvert.SerializeObject(SelectedMinute.Model);
                    await Shell.Current.GoToAsync($"minute/?model={jsonStr}", true);
                }, 
                log =>
                {
                    if (log != null)
                        UserDialogs.Instance.Alert(log);

                    return Task.CompletedTask;
                });
            });
        }

        public Command DeleteMinute
        {
            get { if (_deleteMinute == null) _deleteMinute = new Command(DeleteMinuteExecute); return _deleteMinute; }
        }
        private Command _deleteMinute;
        private void DeleteMinuteExecute()
        {
            logService.LogActionAsync(async () =>
            {
                if (!await UserDialogs.Instance.ConfirmAsync("Esta operação removerá a informação definitivamente. Deseja prosseguir?", "Confirmação", "Sim", "Não"))
                    return;
                
                await SetBusyAsync(async () =>
                {
                    await Task.Delay(300);

                    var topics = dataService.TopicRepository.Find(l => l.IdMinute == SelectedMinute.Model.Id);
                    foreach (var topic in topics)
                    {
                        var information = dataService.InformationRepository.Find(t => t.IdTopic == topic.Id);
                        information.ForEach(i => dataService.InformationRepository.Remove(i));

                        dataService.TopicRepository.Remove(topic);
                    }

                    var people = dataService.PersonRepository.Find(l => l.IdMinute == SelectedMinute.Model.Id);
                    people.ForEach(p => dataService.PersonRepository.Remove(p));

                    dataService.MinuteRepository.Remove(SelectedMinute.Model);

                    Minutes.Remove(SelectedMinute);
                    ClearSelection.Execute(null);
                });
            },
            log =>
            {
                if (log != null)
                    UserDialogs.Instance.Alert(log);

                return Task.CompletedTask;
            });
        }

        public Command ClearSelection
        {
            get { if (_clearSelection == null) _clearSelection = new Command(ClearSelectionExecute); return _clearSelection; }
        }
        private Command _clearSelection;
        private void ClearSelectionExecute()
        {
            // Unselect all minutes
            foreach (var minute in Minutes)
                minute.IsSelected = false;

            // Clear selected minute
            SelectedMinute = null;
        }

        public Command PrintWord
        {
            get { if (_printWord == null) _printWord = new Command(PrintWordExecute); return _printWord; }
        }
        private Command _printWord;
        private void PrintWordExecute()
        {
            logService.LogActionAsync(async () =>
            {
                //Note: it's important to pass SelectedMinute.Model here because at some point the SelectedMinute is becoming a null reference
                //right after approving StoragePermission request
                //TODO: investigate why this happens
                var minute = SelectedMinute.Model;

                if (!await permissionService.RequestStoragePermission())
                    return;

                ShowLoading();
                // Running on a background thread
                _ = Task.Run(() => PrintWordInternal(minute));
            });
        }

        public Command PrintPDF
        {
            get { if (_printPDF == null) _printPDF = new Command(PrintPDFExecute); return _printPDF; }
        }
        private Command _printPDF;
        private void PrintPDFExecute()
        {
            logService.LogActionAsync(async () =>
            {
                //Note: it's important to store SelectedMinute.Model here because the DisplayAlert reinitializes the page,
                //thus making the SelectedMinute a null reference
                var minute = SelectedMinute.Model;

                if (!await permissionService.RequestStoragePermission())
                    return;

                if (!await DisplayAlert("Aviso", "Para gerar um PDF você precisará ver um vídeo antes. Isto ajuda a financiar este aplicativo. Você concorda?\r\n\r\nREQUER INTERNET", "Sim", "Não"))
                    return;

                ShowLoading();
                adService.ShowVideo(AppConsts.AdVideo,
                    // Callback for success (this happens in its own thread and eventually the ad will be closed as well)
                    () => isRewarded = true,
                    // Callback for ad close (this always happens, whether the ad was closed after or before it was successful) 
                    () => PrintPDFInternal(minute),
                    // Callback for failure (this negates the callbacks above)
                    () =>
                    {
                        UserDialogs.Instance.Toast("Conexão falhou. Verifique a internet!");

                        isRewarded = false;

                        ClearSelection.Execute(null);
                        HideLoading();
                    });
            });
        }

        #endregion

        #region Helpers

        private void LoadMinutesInternal()
        {
            var minutes = dataService.MinuteRepository.GetAll();
            var minuteCollection = new List<MinuteElement>();

            var people = dataService.PersonRepository.GetAll();

            foreach (var minute in minutes)
                minuteCollection.Add(new MinuteElement(minute)
                {
                    PeopleQuantity = people.Count(l => l.IdMinute == minute.Id),
                });

            InvokeMainThread(() =>
            {
                SelectedMinute = null;
                Minutes.ReplaceRange(minuteCollection);
            });
        }

        private void PrintWordInternal(Minute minute)
        {
            // This stream will be disposed by the PrintService
            MemoryStream stream = null;

            logService.LogAction(() =>
            {
                byte[] localBytes = GetImageBytesInternal();

                // This stream needs to be disposed eventually. Keep in mind that PrintService calls close the stream internally
                stream = new MemoryStream
                {
                    Position = 0,
                };

                // Generates the document and saves it to a stream
                var localDocument = CreateDocumentInternal(minute, localBytes);
                localDocument.Save(stream, Syncfusion.DocIO.FormatType.Docx);
                localDocument.Close();

                // Removing any special characters not meant for use as file name
                var arquivonome = minute.Name.RemoveSpecialCharacters();
                InvokeMainThread(() =>
                {
                    HideLoading();
                    printService.Print(arquivonome + ".docx", "application/msword", stream);
                });
            },
            log =>
            {
                // Disposing the stream, assuming that the background thread didn't get to the point of disposing it through the PrintService
                try { stream?.Dispose(); }
                catch (ObjectDisposedException) { }

                if (log != null)
                    InvokeMainThread(() =>
                    {
                        HideLoading();
                        UserDialogs.Instance.Toast(log);
                    });
            });
        }

        private void PrintPDFInternal(Minute minute)
        {
            // This stream needs to be disposed. Keep in mind that PrintService disposes it automatically
            MemoryStream stream = null;

            logService.LogAction(() =>
            {
                if (!isRewarded)
                {
                    UserDialogs.Instance.Toast("Você precisa aguardar a recompensa!");

                    isRewarded = false;
                    HideLoading();

                    return;
                }

                // This stream needs to be disposed eventually. Keep in mind that PrintService calls close the stream internally
                stream = new MemoryStream
                {
                    Position = 0
                };

                // Retrieving the bytes for the image selected by the user for the minute
                byte[] localbyte = GetImageBytesInternal();

                // Generating the word document
                WordDocument wordDocument = CreateDocumentInternal(minute, localbyte, true);

                // Convert to pdf document
                var pdfDocument = ConvertToPdf(wordDocument);

                // Disposing the word document and saving it to the stream
                wordDocument.Dispose();
                pdfDocument.Save(stream);
                pdfDocument.Close();

                var fileName = minute.Name.RemoveSpecialCharacters();
                printService.Print($"{fileName}.pdf", "application/msword", stream);

                ClearSelection.Execute(null);
                isRewarded = false;
                HideLoading();
            },
            log =>
            {
                // Disposing the stream, assuming that the background thread didn't get to the point of disposing it through the PrintService
                try { stream?.Dispose(); }
                catch (ObjectDisposedException) { }

                if (log != null)
                    InvokeMainThread(() =>
                    {
                        HideLoading();
                        UserDialogs.Instance.Toast(log);
                    });
            });
        }

        WordDocument CreateDocumentInternal(Minute minute, byte[] localbyte, bool isPDF = false)
        {
            var userName = App.Current.Properties[AppConsts.UserName]?.ToString();
            var organizationName = App.Current.Properties[AppConsts.OrganizationName]?.ToString();
            var fontFamily = App.Current.Properties[AppConsts.FontFamily]?.ToString();
            var fontSize = int.Parse(App.Current.Properties[AppConsts.FontSize]?.ToString());

            var marginLeft = int.Parse(App.Current.Properties[AppConsts.MarginLeft]?.ToString());
            var marginTop = int.Parse(App.Current.Properties[AppConsts.MarginTop]?.ToString());
            var marginRight = int.Parse(App.Current.Properties[AppConsts.MarginRight]?.ToString());
            var marginBottom = int.Parse(App.Current.Properties[AppConsts.MarginBottom]?.ToString());

            WordDocument localDocument = new WordDocument();

            IWSection localSection = localDocument.AddSection();
            localSection.PageSetup.Margins = new MarginsF(marginLeft * 28.35f, marginTop * 28.35f, marginRight * 28.35f, marginBottom * 28.35f);

            IWParagraph localParagraph1 = localSection.AddParagraph();
            localParagraph1.ParagraphFormat.HorizontalAlignment = HorizontalAlignment.Center;

            IWPicture localpicture = localParagraph1.AppendPicture(localbyte);
            localpicture.HeightScale = 20;
            localpicture.WidthScale = 20;

            localSection.AddParagraph();

            IWParagraph localParagraph2 = localSection.AddParagraph();
            localParagraph2.ParagraphFormat.HorizontalAlignment = HorizontalAlignment.Center;
            IWTextRange titleText = localParagraph2.AppendText("ATA DE REUNIÃO" + " - " + organizationName);

            titleText.CharacterFormat.TextColor = Syncfusion.Drawing.Color.ForestGreen;
            titleText.CharacterFormat.Font = new Syncfusion.Drawing.Font(fontFamily, fontSize + 4);

            localSection.AddParagraph();

            // People List
            var people = string.Empty;
            var personList = dataService.PersonRepository.Find(l => l.IdMinute == minute.Id)?.ToList() ?? new List<Person>();
            for (int i = 0; i < personList.Count; i++)
            {
                if (i == 0) people = personList[i].Name;
                else people += ($", {personList[i].Name}");
            }

            // Justifying the paragraph
            IWParagraph localParagraph3 = localSection.AddParagraph();
            localParagraph3.ParagraphFormat.HorizontalAlignment = HorizontalAlignment.Justify;

            IWTextRange textoDescricao = localParagraph3.AppendText($"Reunião da Organização {organizationName}, realizada no dia {minute.Date} às {minute.Start} com a presença de {people}");
            textoDescricao.CharacterFormat.Font = new Syncfusion.Drawing.Font(fontFamily, fontSize);

            localSection.AddParagraph();

            // Justifying the paragraph
            IWParagraph localParagraph4 = localSection.AddParagraph();
            localParagraph4.ParagraphFormat.HorizontalAlignment = HorizontalAlignment.Justify;

            // Paragraph header
            IWTextRange subDescriptionText = localParagraph4.AppendText("A seguinte pauta foi discutida:");
            subDescriptionText.CharacterFormat.Font = new Syncfusion.Drawing.Font(fontFamily, fontSize);

            // Topic and Information List
            var topics = dataService.TopicRepository.Find(l => l.IdMinute == minute.Id)?.ToList() ?? new List<Topic>();
            for (int i = 0; i < topics.Count; i++)
            {
                localSection.AddParagraph();

                // Paragraph Format
                IWParagraph localParagraphTopic = localSection.AddParagraph();
                localParagraphTopic.ListFormat.ApplyDefNumberedStyle();
                localParagraphTopic.ListFormat.ContinueListNumbering();
                localParagraphTopic.ParagraphFormat.HorizontalAlignment = HorizontalAlignment.Justify;

                // Topic Text
                IWTextRange topicText = localParagraphTopic.AppendText($"{topics[i].Text}:");
                topicText.CharacterFormat.Font = new Syncfusion.Drawing.Font(fontFamily, fontSize);
                topicText.CharacterFormat.Bold = true;

                // Information List
                var information = dataService.InformationRepository.Find(l => l.IdTopic == topics[i].Id)?.ToList() ?? new List<Information>();
                foreach (var info in information)
                {
                    // Checking if this information is not empty
                    if (!string.IsNullOrEmpty(info.Text?.Replace("\n\n", string.Empty).Trim()))
                    {
                        IWParagraph localParagraphInformation = localSection.AddParagraph();
                        localParagraphInformation.ParagraphFormat.HorizontalAlignment = HorizontalAlignment.Justify;
                        localParagraphInformation.ListFormat.ApplyDefBulletStyle();

                        // Information Text
                        IWTextRange informationText;
                        // If the text has any line breaks, remove them
                        info.Text = info.Text?.Replace("\n\n", " ");

                        // Checking if the output is PDF
                        if (isPDF)
                            informationText = localParagraphInformation.AppendText($"- {info.Text};");
                        else
                            informationText = localParagraphInformation.AppendText($"{info.Text};");

                        // Text Font
                        informationText.CharacterFormat.Font = new Syncfusion.Drawing.Font(fontFamily, fontSize);
                    }
                }
            }

            localSection.AddParagraph();

            IWParagraph localParagraph5 = localSection.AddParagraph();
            localParagraph5.ParagraphFormat.HorizontalAlignment = HorizontalAlignment.Justify;

            IWTextRange localText = localParagraph5.AppendText($"Finalizada às {minute.End}. Gerada por {userName}.");
            localText.CharacterFormat.FontSize = fontSize;
            localText.CharacterFormat.TextColor = Syncfusion.Drawing.Color.Black;
            localText.CharacterFormat.Font = new Syncfusion.Drawing.Font(fontFamily, fontSize);

            return localDocument;
        }

        byte[] GetImageBytesInternal()
        {
            if (Application.Current.Properties[AppConsts.SelectedMinuteImage]?.ToString() != "0")
            {
                var selectedImage = int.Parse(Application.Current.Properties[AppConsts.SelectedMinuteImage]?.ToString());
                var minuteImage = dataService.MinuteImageRepository.Get(selectedImage);

                return imageService.GetBytesFromPath(minuteImage.Name);
            }
            else
                return imageService.GetBytesFromLogo();
        }

        PdfDocument ConvertToPdf(WordDocument document)
        {
            DocIORenderer render = new DocIORenderer();
            PdfDocument pdfDocument = render.ConvertToPDF(document);
            render.Dispose();

            return pdfDocument;
        }

        #endregion

        #region Initializers

        public override void Initialize()
        {
            base.Initialize();

            LoadMinutesInternal();
        }

        #endregion
    }
}
