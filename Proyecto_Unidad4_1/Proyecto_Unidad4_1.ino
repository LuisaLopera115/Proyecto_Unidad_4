// Arduino ESP32 2 
//CLOCK
#include <Wire.h>
#include <WiFi.h>
#include <WiFiUdp.h>
#define DS3231_I2C_ADDRESS 0x68

uint8_t second, minute, hour, wday, day, month, year;
const char* ssid = "LOPERAGALLO";
const char* password = "43204751";
WiFiUDP udpDevice;
uint16_t localUdpPort = 50002;
uint16_t UDPPort = 50001;
const char* host = "192.168.1.9";
uint32_t previousMillis = 0;
#define ALIVE 1000
#define D0 4
byte packetBuffer[255];
char* datas;
uint8_t arr[19] = {0};

void setup() {
  //------- LED ----------------------------
  pinMode(D0, OUTPUT);     // Initialize the LED_BUILTIN pin as an output
  digitalWrite(D0, HIGH);

  Serial.begin(115200);
  Serial.println();
  //------- WIFI ----------------------------
  Serial.print("Connecting to ");
  Serial.println(ssid);
  WiFi.mode(WIFI_STA);
  WiFi.begin(ssid, password);
  while (WiFi.status() != WL_CONNECTED) {
    delay(500);
    Serial.print(".");
  }
  Serial.println("");
  Serial.println("WiFi connected");
  // Print the IP address
  Serial.println("And the local ip is");
  Serial.println(WiFi.localIP());
  udpDevice.begin(localUdpPort);

  Wire.begin();
}
void SendTask() {
  ReadTime();
  arr[0] = 0x2f;
  arr[1] = 0x72;
  arr[2] = 0x5c;
  arr[3] = 0x00;
  
  arr[4] = 0x2c;
  arr[5] = 0x69;
  arr[6] = 0x69;
  arr[7] = 0x69;
  
  arr[8] = 0x69;
  arr[9] = 0x69;
  arr[10] = 0x69;
  arr[11] = 0x69;
  
  arr[12] = second;
  arr[13] = minute;
  arr[14] = hour;
  arr[15] = wday;
  arr[16] = day;
  arr[17] = month;
  arr[18] = year;
  
  uint32_t currentMillis;
  currentMillis  = millis();

  if ((currentMillis - previousMillis) >= ALIVE) {
    previousMillis = currentMillis;
    PrintTime();
    Serial.println("Encvia");
    udpDevice.beginPacket(host , UDPPort);
    udpDevice.write(arr, 19);
    udpDevice.endPacket();
  }
}
void ReceiveTask() {
  enum class serialStates {Recive, Send, WaitData};
  static auto state = serialStates::Recive;
  switch (state)
  {
    case serialStates::Recive: {
        int packetSize = udpDevice.parsePacket();
        if (packetSize > 0) {
          Serial.print("Received packet of size ");
          Serial.println(packetSize);
          Serial.print("From ");
          IPAddress remoteIp = udpDevice.remoteIP();
          Serial.print(remoteIp);
          Serial.print(", port ");
          Serial.println(udpDevice.remotePort());
          // read the packet into packetBufffer
          int len = udpDevice.read(packetBuffer, packetSize);

          for (int i = 0; i < len; i++ ) {
            Serial.print(packetBuffer[i], HEX);
            Serial.print(" ");
          }
          Serial.println();
          Serial.println("Contents:");
          datas = (char*) packetBuffer;
          Serial.println(datas);

          if (packetBuffer[0] == 0x2f and packetBuffer[1] == 0x63 and packetBuffer[2] == 0x5c) {
            if (packetBuffer[4] == 0x2c and packetBuffer[5] == 0x69 and packetBuffer[6] == 0x69 and packetBuffer[7] == 0x69 and packetBuffer[8] == 0x69 and packetBuffer[9] == 0x69 and packetBuffer[10] == 0x69 and packetBuffer[11] == 0x69) {}
            second = (packetBuffer[12]);
            minute = (packetBuffer[13]);//_minute;
            hour = (packetBuffer[14]);//_hour;
            wday = (packetBuffer[15]);//_wday;
            day = (packetBuffer[16]);//_day;
            month = (packetBuffer[17]);//_month;
            year = (packetBuffer[18]);//_year;
            Configtime();
            PrintTime();
            Serial.println("configuracion reloj");
            state = serialStates::Send;
          }
        }
        break;
      }
    case serialStates::Send: {
        SendTask();
        break;
      }
  }
}

void loop() {
  ReceiveTask();
}

byte decToBcd(byte val) {
  return ( (val / 10 * 16) + (val % 10) );
}

byte bcdToDec(byte val) {
  return ( (val / 16 * 10) + (val % 16) );
}

void Configtime() {
  Wire.beginTransmission(DS3231_I2C_ADDRESS);
  Wire.write(0);
  
  Wire.write(decToBcd(second));
  Wire.write(decToBcd(minute));
  Wire.write(decToBcd(hour));
  Wire.write(decToBcd(wday));
  Wire.write(decToBcd(day));
  Wire.write(decToBcd(month));
  Wire.write(decToBcd(year));
  Wire.endTransmission();
}

void ReadTime() {
  byte temphour;
  Wire.beginTransmission(DS3231_I2C_ADDRESS);
  Wire.write(0);
  Wire.endTransmission();
  Wire.requestFrom(DS3231_I2C_ADDRESS, 7);

  second = bcdToDec(Wire.read() & 0x7f);
  minute = bcdToDec(Wire.read());
  hour = bcdToDec(Wire.read() & 0x3f);
  wday = bcdToDec(Wire.read());
  day = bcdToDec(Wire.read());
  month = bcdToDec(Wire.read());
  year = bcdToDec(Wire.read());
}

void PrintTime() {
  Serial.print(second);
  Serial.print(":");
  Serial.print(minute);
  Serial.print(":");
  Serial.print(hour);

  Serial.print("    ");
  Serial.print(wday);
  Serial.print(" ");
  Serial.print(day);
  Serial.print("/");
  Serial.print(month);
  Serial.print("/");
  Serial.print(year);
  Serial.println();
}
