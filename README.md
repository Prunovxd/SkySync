# **Bluetooth Control Application and Arduino Code**

## Описание проекта

Этот проект включает в себя мобильное приложение, разработанное на **.NET MAUI**, и Arduino код для управления устройствами через Bluetooth. В качестве Bluetooth-модуля используется **JDY-23** с поддержкой NRF-коммуникации. Приложение обеспечивает взаимодействие с Arduino-устройством для управления и мониторинга работы подключенных компонентов.

---

## Функциональность

### **MAUI Приложение**
- Подключение к устройству через **Bluetooth JDY-23**.
- Отправка команд на Arduino для управления компонентами (например, светодиодами, моторами).
- Получение данных от Arduino (например, состояние сенсоров или подтверждение выполнения команд).
- Простой и интуитивно понятный интерфейс для работы.

### **Arduino Код**
- Обработка входящих команд от Bluetooth-приложения.
- Управление периферийными устройствами (например, светодиодами или сервоприводами).
- Отправка текущих данных (например, показаний датчиков) обратно в приложение.

---

## Как использовать

### **MAUI Приложение**
1. Скачайте и установите приложение на устройство.
2. Запустите приложение и выполните следующие шаги:
   - Активируйте Bluetooth на устройстве.
   - В списке доступных устройств выберите модуль **JDY-23**.
   - Подключитесь и начните отправлять команды.
3. Используйте кнопки или элементы интерфейса для управления.

### **Arduino**
1. Загрузите предоставленный скетч в Arduino через **Arduino IDE**.
2. Убедитесь, что модуль JDY-23 подключен к Arduino:
   - **TX** модуля подключен к **RX** Arduino.
   - **RX** модуля подключен к **TX** Arduino.
   - Подключите питание модуля (**VCC** и **GND**).
3. Включите питание Arduino.
4. Подключитесь к модулю JDY-23 через мобильное приложение.

---

## Настройка

### **MAUI Приложение**
- Убедитесь, что проект настроен под вашу платформу:
  - Android: добавьте разрешения для Bluetooth и доступ к местоположению.
  - iOS: настройте Bluetooth-функции в файле `Info.plist`.
- Для сборки проекта используйте **Visual Studio** с установленным MAUI Workload.

### **Arduino**
- Загрузите последнюю версию **Arduino IDE**.
- Убедитесь, что выбран правильный COM-порт и плата Arduino (например, **Arduino Uno**).
- Настройте скорость последовательного соединения в коде на **9600** (или другое значение, если используется другая скорость).

---

## Требования

### Оборудование:
- Arduino (например, Uno, Mega или Nano).
- Bluetooth-модуль **JDY-23**.
- Периферийные устройства (например, светодиоды, датчики).

### Программное обеспечение:
- **.NET MAUI SDK**.
- **Visual Studio** (для разработки MAUI-приложения).
- **Arduino IDE** (для работы с Arduino кодом).

---

## Заметки
- Убедитесь, что используете NRF-библиотеки для связи с JDY-23.
- Если соединение не устанавливается, проверьте питание модуля и подключение RX/TX.
- Для отладки можно использовать последовательный монитор в Arduino IDE.

---

## Лицензия
Проект распространяется под лицензией MIT.