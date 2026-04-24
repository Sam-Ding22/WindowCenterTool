using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

namespace WindowResizerApp;

internal sealed class SettingsForm : Form
{
    private readonly CheckBox _launchOnLoginCheckBox;
    private readonly CheckBox _showNotificationsCheckBox;
    private readonly Dictionary<string, ComboBox> _modifierInputs = new();
    private readonly Dictionary<string, ComboBox> _keyInputs = new();

    public SettingsForm(AppSettings settings)
    {
        Text = "窗口居中工具 / Window Center Tool";
        FormBorderStyle = FormBorderStyle.FixedDialog;
        MaximizeBox = false;
        MinimizeBox = false;
        StartPosition = FormStartPosition.CenterScreen;
        ClientSize = new Size(620, 280);

        var root = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 4,
            RowCount = 6,
            Padding = new Padding(12),
            AutoSize = true
        };
        root.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 35));
        root.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 25));
        root.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 15));
        root.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 25));

        _launchOnLoginCheckBox = new CheckBox
        {
            Text = "开机自启 / Launch on login",
            Checked = settings.LaunchOnLogin,
            AutoSize = true
        };

        _showNotificationsCheckBox = new CheckBox
        {
            Text = "显示提示 / Show notifications",
            Checked = settings.ShowNotifications,
            AutoSize = true
        };

        root.Controls.Add(_launchOnLoginCheckBox, 0, 0);
        root.SetColumnSpan(_launchOnLoginCheckBox, 4);
        root.Controls.Add(_showNotificationsCheckBox, 0, 1);
        root.SetColumnSpan(_showNotificationsCheckBox, 4);

        root.Controls.Add(CreateHeaderLabel("动作 / Action"), 0, 2);
        root.Controls.Add(CreateHeaderLabel("默认 / Default"), 1, 2);
        root.Controls.Add(CreateHeaderLabel("修饰键 / Modifier"), 2, 2);
        root.Controls.Add(CreateHeaderLabel("主按键 / Primary key"), 3, 2);

        var row = 3;
        foreach (var action in HotkeyActions.Ordered)
        {
            var binding = settings.Hotkeys.TryGetValue(action, out var existing)
                ? existing
                : new HotkeyBinding("Alt", 0x41);

            var actionLabel = new Label
            {
                Text = HotkeyActions.GetDisplayName(action),
                AutoSize = true,
                Anchor = AnchorStyles.Left
            };

            var defaultLabel = new Label
            {
                Text = HotkeyActions.GetDefaultShortcutText(action),
                AutoSize = true,
                Anchor = AnchorStyles.Left
            };

            var modifierComboBox = new ComboBox
            {
                DropDownStyle = ComboBoxStyle.DropDownList,
                DataSource = HotkeyOptions.Modifiers.ToList(),
                SelectedItem = binding.Modifier
            };

            var keyComboBox = new ComboBox
            {
                DropDownStyle = ComboBoxStyle.DropDownList,
                DataSource = HotkeyOptions.Keys.ToList(),
                SelectedItem = HotkeyOptions.GetKeyOption(binding.VirtualKey)
            };

            _modifierInputs[action] = modifierComboBox;
            _keyInputs[action] = keyComboBox;

            root.Controls.Add(actionLabel, 0, row);
            root.Controls.Add(defaultLabel, 1, row);
            root.Controls.Add(modifierComboBox, 2, row);
            root.Controls.Add(keyComboBox, 3, row);
            row++;
        }

        var buttonPanel = new FlowLayoutPanel
        {
            Dock = DockStyle.Bottom,
            FlowDirection = FlowDirection.RightToLeft,
            Padding = new Padding(12)
        };

        var saveButton = new Button
        {
            Text = "Save",
            DialogResult = DialogResult.OK,
            AutoSize = true
        };
        saveButton.Click += SaveButtonOnClick;

        var cancelButton = new Button
        {
            Text = "Cancel",
            DialogResult = DialogResult.Cancel,
            AutoSize = true
        };

        buttonPanel.Controls.Add(saveButton);
        buttonPanel.Controls.Add(cancelButton);

        Controls.Add(root);
        Controls.Add(buttonPanel);
    }

    public AppSettings? Result { get; private set; }

    private static Label CreateHeaderLabel(string text)
    {
        return new Label
        {
            Text = text,
            AutoSize = true,
            Font = new Font(SystemFonts.DefaultFont, FontStyle.Bold),
            Margin = new Padding(0, 8, 0, 4)
        };
    }

    private void SaveButtonOnClick(object? sender, EventArgs e)
    {
        var hotkeys = new Dictionary<string, HotkeyBinding>();
        var combinations = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        foreach (var action in HotkeyActions.Ordered)
        {
            var modifier = _modifierInputs[action].SelectedItem?.ToString() ?? string.Empty;
            var key = _keyInputs[action].SelectedItem as KeyOption;

            if (string.IsNullOrWhiteSpace(modifier) || key is null)
            {
                MessageBox.Show(this, "请为每个动作选择修饰键和主按键。\nPlease select a modifier and primary key for every action.", "Invalid Settings");
                DialogResult = DialogResult.None;
                return;
            }

            var combinationKey = $"{modifier}:{key.VirtualKey}";
            if (!combinations.Add(combinationKey))
            {
                MessageBox.Show(this, "每个动作的主快捷键必须唯一。\nEach action must use a unique primary hotkey.", "Duplicate Hotkeys");
                DialogResult = DialogResult.None;
                return;
            }

            hotkeys[action] = new HotkeyBinding(modifier, key.VirtualKey);
        }

        Result = new AppSettings
        {
            LaunchOnLogin = _launchOnLoginCheckBox.Checked,
            ShowNotifications = _showNotificationsCheckBox.Checked,
            Hotkeys = hotkeys
        };

        DialogResult = DialogResult.OK;
        Close();
    }
}
