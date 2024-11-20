#include <SPI.h>
#include <TinyGPSPlus.h>
#include <RF24.h>
#include <SoftwareSerial.h>

TinyGPSPlus gps;
SoftwareSerial gpsSerial(3, 4); // RX на пин 3, TX на пин 4
RF24 radio(9, 10); // CE, CSN
unsigned long previousMillis = 0;
const long interval = 100; // Интервал в миллисекундах
const uint64_t address = 0xF0F0F0F0E1LL; // Адрес для передачи

// Данные для отправки
String text = "NULL";
String BTText = "NULL";
String CordsX = "55.727980";
String CordsY = "37.607092";
String DestinationX = "NULL";
String DestinationY = "NULL";
String Customer_FullName = "NULL";
String Weight = "NULL";
String Price = "NULL";
String Comments = "NULL";
String Address = "NULL";

int sendState = 0; // Текущее состояние отправки
bool success = false;

void setup() {
  Serial.begin(9600);
  gpsSerial.begin(9600);

  radio.begin(); // Инициализация модуля
  radio.openWritingPipe(address); // Настройка канала для отправки
  radio.setPALevel(RF24_PA_MAX); // Установите уровень мощности
  radio.setDataRate(RF24_250KBPS);
  radio.setRetries(3, 5); // 3 попытки отправки, интервал 5*250мкс
  radio.setCRCLength(RF24_CRC_16); // 16-битный CRC
  radio.setPayloadSize(32);
  radio.setChannel(33); // Настройка канала
  Serial.println("NRF24L01 Initialized");
}

void parseMessage(String text, String parts[], int &partCount) {
  int startIndex = 0;
  int semicolonIndex = 0;
  partCount = 0;

  while ((semicolonIndex = text.indexOf(';', startIndex)) != -1) {
    parts[partCount++] = text.substring(startIndex, semicolonIndex);
    startIndex = semicolonIndex + 1; // Переходим к следующей части
  }

  if (startIndex < text.length()) {
    parts[partCount++] = text.substring(startIndex); // Добавляем последнюю часть
  }
}

void loop() {
  // Обработка входящих данных Bluetooth
  if (Serial.available()) {
    String checker = Serial.readString();
    if ((int)checker[checker.length() - 1] != 10 && (int)checker[checker.length() - 2] != 13) {
      BTText = checker;
      const int maxParts = 10; // Максимальное количество частей
      String parts[maxParts];
      int partCount = 0;
      parseMessage(BTText, parts, partCount);
      DestinationX = parts[0];
      DestinationY = parts[1];
      Address = parts[2];
      Customer_FullName = parts[3];
      Weight = parts[4];
      Price = parts[5];
      Comments = parts[6];
      Serial.println(BTText);
    }
  }

  // Обработка данных GPS
  if (gpsSerial.available()) {
    gps.encode(gpsSerial.read());
  }
  if (gps.location.isUpdated()) {
    Serial.println(gps.location.lat(), 6);
    Serial.println(gps.location.lng(), 6);
    CordsX = String(gps.location.lat());
    CordsY = String(gps.location.lng());
  }

  // Отправка данных с интервалом
  unsigned long currentMillis = millis();
  if (currentMillis - previousMillis >= interval) {
    previousMillis = currentMillis;
    sendDataStep();
  }
}

void sendDataStep() {
  switch (sendState) {
    case 0:
      text = "!" + CordsX + String(random(61)) + ";";
      break;
    case 1:
      text = CordsY + String(random(61)) + ";";
      break;
    case 2:
      text = DestinationX + ";";
      break;
    case 3:
      text = DestinationY + ";";
      break;
    case 4:
      text = Address + ";";
      break;
    case 5:
      text = Customer_FullName + ";";
      break;
    case 6:
      text = Weight + ";";
      break;
    case 7:
      text = Price + ";";
      break;
    case 8:
      text = Comments + ";";
      break;
    default:
      sendState = 0;
      return; // Все данные отправлены
  }

  success = radio.write(text.c_str(), text.length());
  if (success) {
    Serial.println("Data sent successfully: " + text);
  } else {
    Serial.println("Failed to send data: " + text);
  }

  sendState++; // Переход к следующему состоянию
}
