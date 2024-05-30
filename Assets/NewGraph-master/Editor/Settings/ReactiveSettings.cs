using System;
using UnityEditor.UIElements;

namespace NewGraph {

    using static GraphSettingsSingleton;

    public class ReactiveSettings {
        public ReactiveSettings(Action OnSettingsChanged) {
            this.OnSettingsChanged = OnSettingsChanged;
            SettingsChanged(null);
            Settings.ValueChanged -= SettingsChanged;
            Settings.ValueChanged += SettingsChanged;
        }

        private Action OnSettingsChanged;

        private void SettingsChanged(SerializedPropertyChangeEvent evt) {
            OnSettingsChanged();
        }

        public static void Create(ref ReactiveSettings instanceField, Action OnSettingsChanged) {
            if (instanceField == null) {
                instanceField = new ReactiveSettings(OnSettingsChanged);
            }
        }
    }
}
