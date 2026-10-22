# Meta Quest voice input setup

The Quest build includes the offline English `vosk-model-small-en-us-0.15` model and an ARM64 native plugin. Hold the left trigger for 0.65 seconds to start an English voice message. A short left-trigger press still lights nearby candles. Microphone input is processed locally when no Wit.ai configuration is assigned, avoiding the Quest platform error `dictation service not available`. Recognized output is limited to English letters, digits and basic punctuation. The model comes from the [official Vosk model catalog](https://alphacephei.com/vosk/models) under Apache 2.0. The bundled [Noto Sans CJK SC font](https://github.com/notofonts/noto-cjk) supports existing interface text under the SIL Open Font License.

Allow microphone access when prompted on the headset. The first launch extracts the 40 MB English model to app storage; the status message shows when the model is ready and when recording starts. Later attempts reuse it. To use a cloud model, configure Wit.ai:

1. Create a Wit.ai app with the language used for player messages.
2. In Unity, open **Meta > Voice SDK > Settings** and link the app using its Server Access Token. Save the resulting `WitConfiguration` asset.
3. Either assign that asset to the `Wit Configuration` field of `SkyQuestCommunication` in both `SkyDesktop` and `SkyQuest3S`, or save it as `Assets/Resources/SkyQuestWitConfig.asset` so both scenes load it automatically.
4. Rebuild the Quest APK and allow microphone access. In the Unity Editor, voice input still needs Wit because the bundled native Vosk plugin targets Android ARM64.

Keep the Wit.ai token in Unity. Do not paste it into chat messages or commit the configuration asset to a public repository.
