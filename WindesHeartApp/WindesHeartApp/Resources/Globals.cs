﻿using System.Collections.Generic;
using WindesHeartApp.Data.Interfaces;
using WindesHeartApp.Services;
using WindesHeartApp.ViewModels;
using Xamarin.Forms;

namespace WindesHeartApp.Resources
{
    public static class Globals
    {
        public static HeartRatePageViewModel HeartrateviewModel;
        public static SamplesService SamplesService { get; private set; }
        public static DevicePageViewModel DevicePageViewModel;
        public static HomePageViewModel HomepageviewModel;
        public static SettingsPageViewmodel SettingspageviewModel;
        public static double ScreenHeight { get; set; }
        public static double ScreenWidth { get; set; }
        public static Color PrimaryColor { get; set; } = Color.FromHex("#96d1ff");
        public static Color SecondaryColor { get; set; } = Color.FromHex("#53b1ff");
        public static Color LightTextColor { get; set; } = Color.FromHex("#999999");
        public static IStepsRepository StepsRepository { get; set; }
        public static ISleepRepository SleepRepository { get; set; }
        public static ISettingsRepository SettingsRepository { get; set; }
        public static IHeartrateRepository HeartrateRepository { get; set; }
        public static float DailyStepsGoal { get; internal set; }
        public static Dictionary<string, Color> ColorDictionary;
        public static Dictionary<string, string> FormatDictionary;
        public static StepsViewModel StepsViewModel;
        public static SleepPageViewModel SleepViewModel;

        public static void BuildGlobals(IHeartrateRepository heartrateRepository, ISleepRepository sleepRepository, IStepsRepository stepsRepository, ISettingsRepository settingsRepository)
        {
            DailyStepsGoal = 1000;
            StepsRepository = stepsRepository;
            SleepRepository = sleepRepository;
            SettingsRepository = settingsRepository;
            HeartrateRepository = heartrateRepository;
            HeartrateviewModel = new HeartRatePageViewModel(HeartrateRepository);
            SamplesService = new SamplesService(HeartrateRepository, StepsRepository, SleepRepository);
            StepsViewModel = new StepsViewModel(StepsRepository);
            SettingspageviewModel = new SettingsPageViewmodel(SettingsRepository);
            SleepViewModel = new SleepPageViewModel(sleepRepository);
            DevicePageViewModel = new DevicePageViewModel();
            HomepageviewModel = new HomePageViewModel();
            ColorDictionary = new Dictionary<string, Color>
            {
                { "Aqua", Color.Aqua},
                { "Black", Color.Black},
                { "LightBlue (Default)", Color.FromHex("#96d1ff")}, { "Fucshia", Color.Fuchsia },
                { "Gray", Color.Gray }, { "Green", Color.Green },
                { "Lime", Color.Lime }, { "Maroon", Color.Maroon },
                { "Navy", Color.Navy }, { "Olive", Color.Olive },
                { "Purple", Color.Purple }, { "Red", Color.Red },
                { "Silver", Color.Silver }, { "Teal", Color.Teal },
                { "White", Color.White }, { "Yellow", Color.Yellow }
            };
            FormatDictionary = new Dictionary<string, string>
            {
                {"NL", "nl-NL"},
                {"EN", "en-EN"},
                {"DU", "du-DU"}
            };
        }
    }
}