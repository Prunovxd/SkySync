using Plugin.BLE;
using Plugin.BLE.Abstractions.Contracts;
using System;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Handlers;
using Microsoft.Maui.Controls.Compatibility;
using Plugin.BLE.Abstractions.EventArgs;
using System.Runtime.CompilerServices;

namespace BluetoothApp
{
    public partial class MainPage : ContentPage
    {
        private readonly IAdapter _bluetoothAdapter;
        private IDevice _connectedDevice;
        private ICharacteristic _writeCharacteristic;
        private WebView _webView;
        private Entry latitudeEntry;
        private Entry longitudeEntry;
        private Entry destinationEntry;
        private Entry recipientNameEntry;
        private Entry weightEntry;
        private Entry priceEntry;
        private Entry cargoInfoEntry;
        private Microsoft.Maui.Controls.StackLayout stackLayout;
        public MainPage()
        {
            InitializeComponent();
            _bluetoothAdapter = CrossBluetoothLE.Current.Adapter;
            SetupUI();
        }

        private void SetupUI()
        {
            var connectButton = new Button { Text = "Connect", HorizontalOptions = LayoutOptions.Center };
            connectButton.Clicked += ConnectButton_Clicked;

            var disconnectButton = new Button { Text = "Disconnect", HorizontalOptions = LayoutOptions.Center };
            disconnectButton.Clicked += DisconnectButton_Clicked;

            var sendButton = new Button { Text = "Send", HorizontalOptions = LayoutOptions.Center };
            sendButton.Clicked += SendButton_Clicked;

            latitudeEntry = new Entry { Placeholder = "Широта", HorizontalOptions = LayoutOptions.Center };
            longitudeEntry = new Entry { Placeholder = "Долгота", HorizontalOptions = LayoutOptions.Center };
            destinationEntry = new Entry { Placeholder = "Адрес назначения", HorizontalOptions = LayoutOptions.Center };
            recipientNameEntry = new Entry { Placeholder = "ФИО получателя", HorizontalOptions = LayoutOptions.Center };
            weightEntry = new Entry { Placeholder = "Вес груза (кг)", Keyboard = Keyboard.Numeric, HorizontalOptions = LayoutOptions.Center };
            priceEntry = new Entry { Placeholder = "Цена груза (руб)", Keyboard = Keyboard.Numeric, HorizontalOptions = LayoutOptions.Center };
            cargoInfoEntry = new Entry { Placeholder = "Информация о грузе", MaxLength = 32, HorizontalOptions = LayoutOptions.Center };


            var openMapButton = new Button { Text = "Карта", HorizontalOptions = LayoutOptions.Center};
            openMapButton.Clicked += OpenMapButton_Clicked;

            stackLayout = new Microsoft.Maui.Controls.StackLayout
            {
                Children = { latitudeEntry,
                    longitudeEntry,
                    destinationEntry,
                    recipientNameEntry,
                    weightEntry,
                    priceEntry,
                    cargoInfoEntry,
                    connectButton,
                    sendButton,
                    disconnectButton,
                    openMapButton
 },
                VerticalOptions = LayoutOptions.Center,
                HorizontalOptions = LayoutOptions.Center,
                Spacing = 5
            };

            Content = stackLayout;
        }

        // Открытие карты Google в WebView
        private void OpenMapButton_Clicked(object sender, EventArgs e)
        {
            // Убедитесь, что WebView создается и правильно добавляется в UI
            _webView = new WebView
            {
                Source = new UrlWebViewSource
                {
                    // Для Android
                    Url = "file:///android_asset/map.html" // Путь к HTML файлу для Android
                },
                VerticalOptions = LayoutOptions.FillAndExpand,
                HorizontalOptions = LayoutOptions.FillAndExpand
            };
            // Здесь заменяем текущий контент страницы на WebView
                this.Content = _webView;
            _webView.Navigating += (s, ev) =>
            {
                // Слушаем скрипты, переданные из JavaScript в C#
                try
                {
                    if (ev.Url.Contains("setCoordinates"))
                    {
                        ev.Cancel = true;
                        var query = ev.Url.Split('?').ElementAtOrDefault(1); // Получаем часть после '?'
                        if (query != null)
                        {
                            var coordArray = query.Split(',');
                                latitudeEntry.Text = coordArray[0];
                                longitudeEntry.Text = coordArray[1];
                                Content = stackLayout; // Возвращаем основной интерфейс
                        }
                        else
                        {
                            DisplayAlert("Error", "No coordinates found in URL.", "OK");
                        }
                    }
                }
                catch (Exception ex)
                {
                    DisplayAlert("Error", $"Error parsing coordinates: {ex.Message}", "OK");
                }
            };


        }
        public void SetCoordinates(double latitude, double longitude)
        {
            // Заполняем поля широты и долготы
            latitudeEntry.Text = latitude.ToString();
            longitudeEntry.Text = longitude.ToString();
        }

        private async Task StartScanningForDevices()
        {
            _bluetoothAdapter.DeviceDiscovered += DeviceDiscoveredHandler;

            try
            {
                await _bluetoothAdapter.StartScanningForDevicesAsync();
            }
            catch (Exception ex)
            {
                await DisplayAlert("Error", $"Scanning failed: {ex.Message}", "OK");
                _bluetoothAdapter.DeviceDiscovered -= DeviceDiscoveredHandler;
                return;
            }
        }
        private async void DeviceDiscoveredHandler(object sender, DeviceEventArgs e)
        {
            if (e.Device.Name == "JDY-23")
            {
                await _bluetoothAdapter.StopScanningForDevicesAsync();
                _bluetoothAdapter.DeviceDiscovered -= DeviceDiscoveredHandler;
                await Device.InvokeOnMainThreadAsync(async () => await ConnectToDeviceAsync(e.Device));
            }
        }

        private async Task ConnectToDeviceAsync(IDevice device)
        {
            if (device == null)
            {
                await DisplayAlert("Error", "Device not found.", "OK");
                return;
            }

            try
            {
                await _bluetoothAdapter.ConnectToDeviceAsync(device);
                _connectedDevice = device;

                var service = await device.GetServiceAsync(Guid.Parse("0000ffe0-0000-1000-8000-00805f9b34fb"));

                if (service != null)
                {
                    _writeCharacteristic = await service.GetCharacteristicAsync(Guid.Parse("0000ffe1-0000-1000-8000-00805f9b34fb"));

                    if (_writeCharacteristic != null && _writeCharacteristic.CanWrite)
                    {
                        await DisplayAlert("Success", "Connected to device!", "OK");
                    }
                    else
                    {
                        await DisplayAlert("Error", "Characteristic not writable or not found.", "OK");
                    }
                }
                else
                {
                    await DisplayAlert("Error", "Service not found.", "OK");
                }
            }
            catch (Exception ex)
            {
                await DisplayAlert("Error", $"Connection failed: {ex.Message}", "OK");
            }
        }


        private async Task SendTextToArduino(string text)
        {
            if (_writeCharacteristic != null && _writeCharacteristic.CanWrite)
            {
                var data = Encoding.UTF8.GetBytes(text);
                const int maxLength = 20; // Максимальная длина пакета Bluetooth
                int offset = 0;

                while (offset < data.Length)
                {
                    var chunk = data.Skip(offset).Take(maxLength).ToArray();
                    await _writeCharacteristic.WriteAsync(chunk);
                    offset += maxLength;
                }
                await DisplayAlert("Success", $"Data sent: {text}", "OK");
            }
            else
            {
                await DisplayAlert("Error", "Characteristic is not writable.", "OK");
            }
        }


        private async void ConnectButton_Clicked(object sender, EventArgs e)
        {
            await StartScanningForDevices();
        }

        private async void SendButton_Clicked(object sender, EventArgs e)
        {
            var latitude = latitudeEntry.Text;
            var longitude = longitudeEntry.Text;
            var destination = destinationEntry.Text;
            var recipientName = recipientNameEntry.Text;
            var weight = weightEntry.Text;
            var price = priceEntry.Text;
            var cargoInfo = cargoInfoEntry.Text;

            if (string.IsNullOrWhiteSpace(latitude) || string.IsNullOrWhiteSpace(longitude) ||
                string.IsNullOrWhiteSpace(destination) || string.IsNullOrWhiteSpace(recipientName) ||
                string.IsNullOrWhiteSpace(weight) || string.IsNullOrWhiteSpace(price) || string.IsNullOrWhiteSpace(cargoInfo))
            {
                await DisplayAlert("Error", "All fields must be filled.", "OK");
                return;
            }

            var textToSend = $"{latitude};{longitude};{destination};{recipientName};{weight};{price};{cargoInfo}";
            await SendTextToArduino(textToSend);

        }

        private async void DisconnectButton_Clicked(object sender, EventArgs e)
        {
            if (_connectedDevice != null)
            {
                await _bluetoothAdapter.DisconnectDeviceAsync(_connectedDevice);
                await DisplayAlert("Disconnected", "Device disconnected successfully.", "OK");
                _connectedDevice = null;
                _writeCharacteristic = null;
            }
            else
            {
                await DisplayAlert("Error", "No device connected.", "OK");
            }
        }
    }
}
