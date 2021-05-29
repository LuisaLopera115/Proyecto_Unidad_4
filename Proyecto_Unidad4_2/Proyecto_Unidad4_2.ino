#include <Wire.h>
#include <SPI.h>
#include <Adafruit_Sensor.h>
#include <Adafruit_BME280.h>

#define BME_SCK 18
#define BME_MISO 19
#define BME_MOSI 23
#define BME_CS 5
#define SEALEVELPRESSURE_HPA (1013.25)

Adafruit_BME280 bme(BME_CS);

#include <WiFi.h>
#include <WiFiUdp.h>

const char* ssid = "LOPERAGALLO";
const char* password = "43204751";
WiFiUDP udpDevice;
uint16_t localUdpPort = 50009;
uint16_t UDPPort = 50008;
const char* host = "192.168.1.9";
#define SERIALMESSAGESIZE 3
uint32_t previousMillis = 0;
#define ALIVE 1000
#define D0 17
uint8_t number;
uint8_t arr[12] = {0};

void setup() {
  unsigned status;
  Serial.begin(115200);
  Serial.print("Connecting to ");
  Serial.println(ssid);
  
  WiFi.mode(WIFI_STA);
  WiFi.begin(ssid, password);

  status = bme.begin();
  
  if (!status) {
    while (1) delay(10);
  }
  while (WiFi.status() != WL_CONNECTED) {
    delay(500);
    Serial.print(".");
  }
  Serial.println("");
  Serial.println("WiFi connected");
  // Print the IP address
  Serial.println(WiFi.localIP());
  udpDevice.begin(localUdpPort);
}

void loop() {
  SendUnity();
}

void SendUnity() {
  arr[0]= 0x2f;
  arr[1]= 0x74;
  arr[2]= 0x5c;
  arr[3]= 0x30;
  arr[4]= 0x2c;
  arr[5]= 0x66;
  arr[6]= 0x30;
  arr[7]= 0x30;
  float numTem = bme.readTemperature();
  memcpy(arr + 8, (uint8_t *)&numTem, 4);
  
  for(int i =0; i< 13; i++){Serial.println(arr[i],HEX);}
  Serial.println();
  Serial.println();
  
  uint32_t currentMillis;
  currentMillis  = millis();

  if ((currentMillis - previousMillis) >= ALIVE) {
    previousMillis = currentMillis;
    Serial.println(numTem);
    udpDevice.beginPacket(host , UDPPort);
    udpDevice.write(arr, 12);
    Serial.println("enviado");
    udpDevice.endPacket();
  }
}
