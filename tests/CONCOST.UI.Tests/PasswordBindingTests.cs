using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls;
using CONCOST.UI.AttachedProperties;
using CONCOST.UI.Controls;
using Xunit;

namespace CONCOST.UI.Tests;

/// <summary>
/// How well a password field binds: <see cref="PasswordBox"/> is the one WPF control that cannot
/// expose its value as a dependency property, so the library mirrors it through
/// <see cref="PasswordBoxHelper"/>, and <see cref="PasswordInput"/> wraps it in a real control.
/// </summary>
public class PasswordBindingTests
{
    private static void RunOnSTA(Action action) => WpfApplicationHost.Run(action);

    public class Login : INotifyPropertyChanged
    {
        private string _password = string.Empty;

        public string Password
        {
            get => _password;
            set
            {
                if (_password == value) return;
                _password = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Password)));
            }
        }

        public event PropertyChangedEventHandler? PropertyChanged;
    }

    private static Window Host(FrameworkElement content)
    {
        var window = new Window
        {
            Width = 320,
            Height = 200,
            Content = content,
            WindowStyle = WindowStyle.None,
            ShowInTaskbar = false
        };

        window.Show();
        window.UpdateLayout();
        return window;
    }

    // ---------------------------------------------------------------- PasswordBox

    [Fact]
    public void PasswordBox_pushes_typing_back_to_the_view_model()
    {
        RunOnSTA(() =>
        {
            var vm = new Login();
            var box = new PasswordBox();
            PasswordBoxHelper.SetAttach(box, true);
            box.DataContext = vm;
            box.SetBinding(PasswordBoxHelper.PasswordProperty, nameof(Login.Password));

            var window = Host(box);

            box.Password = "s3cret";
            WpfApplicationHost.Pump();

            Assert.Equal("s3cret", vm.Password);

            window.Close();
        });
    }

    [Fact]
    public void PasswordBox_shows_the_view_model_value()
    {
        RunOnSTA(() =>
        {
            var vm = new Login { Password = "initial" };
            var box = new PasswordBox();
            PasswordBoxHelper.SetAttach(box, true);
            box.DataContext = vm;
            box.SetBinding(PasswordBoxHelper.PasswordProperty, nameof(Login.Password));

            var window = Host(box);

            Assert.Equal("initial", box.Password);
            Assert.Equal(7, PasswordBoxHelper.GetPasswordLength(box));

            window.Close();
        });
    }

    [Fact]
    public void PasswordBox_binding_is_two_way_without_asking()
    {
        RunOnSTA(() =>
        {
            var vm = new Login();
            var box = new PasswordBox();
            PasswordBoxHelper.SetAttach(box, true);
            box.DataContext = vm;

            // No Mode=TwoWay: the attached property declares BindsTwoWayByDefault.
            box.SetBinding(PasswordBoxHelper.PasswordProperty, nameof(Login.Password));

            var window = Host(box);
            box.Password = "typed";
            WpfApplicationHost.Pump();

            Assert.Equal("typed", vm.Password);

            window.Close();
        });
    }

    [Fact]
    public void PasswordBox_follows_the_view_model_when_it_changes()
    {
        RunOnSTA(() =>
        {
            var vm = new Login();
            var box = new PasswordBox();
            PasswordBoxHelper.SetAttach(box, true);
            box.DataContext = vm;
            box.SetBinding(PasswordBoxHelper.PasswordProperty, nameof(Login.Password));

            var window = Host(box);

            vm.Password = "from vm";
            Assert.Equal("from vm", box.Password);

            vm.Password = string.Empty;
            Assert.Equal(string.Empty, box.Password);

            window.Close();
        });
    }

    // ---------------------------------------------------------------- PasswordInput

    [Fact]
    public void PasswordInput_pushes_typing_back_to_the_view_model()
    {
        RunOnSTA(() =>
        {
            var vm = new Login();
            var input = new PasswordInput { DataContext = vm };
            input.SetBinding(PasswordInput.PasswordProperty, nameof(Login.Password));

            var window = Host(input);

            var box = (PasswordBox)input.Template.FindName("PART_PasswordBox", input);
            box.Password = "p@ss";

            Assert.Equal("p@ss", vm.Password);

            window.Close();
        });
    }

    [Fact]
    public void PasswordInput_shows_the_view_model_value()
    {
        RunOnSTA(() =>
        {
            var vm = new Login { Password = "seeded" };
            var input = new PasswordInput { DataContext = vm };
            input.SetBinding(PasswordInput.PasswordProperty, nameof(Login.Password));

            var window = Host(input);

            var box = (PasswordBox)input.Template.FindName("PART_PasswordBox", input);
            var text = (TextBox)input.Template.FindName("PART_TextBox", input);

            Assert.Equal("seeded", box.Password);
            Assert.Equal("seeded", text.Text);

            window.Close();
        });
    }

    [Fact]
    public void PasswordInput_keeps_the_reveal_text_box_in_sync()
    {
        RunOnSTA(() =>
        {
            var vm = new Login();
            var input = new PasswordInput { DataContext = vm };
            input.SetBinding(PasswordInput.PasswordProperty, nameof(Login.Password));

            var window = Host(input);

            var box = (PasswordBox)input.Template.FindName("PART_PasswordBox", input);
            var text = (TextBox)input.Template.FindName("PART_TextBox", input);

            input.ShowPassword = true;
            text.Text = "visible";
            WpfApplicationHost.Pump();

            Assert.Equal("visible", vm.Password);
            Assert.Equal("visible", box.Password);

            input.ShowPassword = false;
            box.Password = "hidden";
            WpfApplicationHost.Pump();

            Assert.Equal("hidden", vm.Password);
            Assert.Equal("hidden", text.Text);

            window.Close();
        });
    }

    [Fact]
    public void PasswordInput_follows_the_view_model_when_it_changes()
    {
        RunOnSTA(() =>
        {
            var vm = new Login();
            var input = new PasswordInput { DataContext = vm };
            input.SetBinding(PasswordInput.PasswordProperty, nameof(Login.Password));

            var window = Host(input);
            var box = (PasswordBox)input.Template.FindName("PART_PasswordBox", input);

            vm.Password = "remote";
            Assert.Equal("remote", box.Password);

            vm.Password = string.Empty;
            Assert.Equal(string.Empty, box.Password);

            window.Close();
        });
    }
}
