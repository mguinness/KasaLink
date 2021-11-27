 # Kasa Link
[Kasa Smart](https://www.kasasmart.com/) is a subsidiary of [TP-Link](https://www.tp-link.com/) which manufactures smart home products like plugs and bulbs.  Several of these products use WiFi to allow remote control either locally or via the cloud.  This project is a console application that allows control of these devices from the command line and runs under Windows, OSX or Linux.

## Introduction

This project builds upon the work done by softScheck to reverse engineer the HS110 smart plug.  They discovered that among the open ports on the device is TCP port 9999 that accepts encrypted [JSON](https://en.wikipedia.org/wiki/JSON) to control the device.

The encryption is a rudimentary [autokey cipher](https://en.wikipedia.org/wiki/Autokey_cipher) using XOR and a hardcoded starting key of 171.  Each request and response is preceded by four bytes in network byte order containing the payload length.

## Configuration
Before using the application you need to edit the `config.ini` file to define your devices and commands.  Add each device that you want to address along with the commands that are supported by each device. The key names can be changed to suit your own environment and are used as the parameters for the application from the command line.

```INI
[Devices]
Plug=192.168.1.10
Bulb=192.168.1.11

[Commands]
PlugOn={"system":{"set_relay_state":{"state":1}}}
PlugOff={"system":{"set_relay_state":{"state":0}}}
BulbOn={"smartlife.iot.smartbulb.lightingservice":{"transition_light_state":{"on_off":1}}}
BulbOff={"smartlife.iot.smartbulb.lightingservice":{"transition_light_state":{"on_off":0}}}
```

## Usage
The following details the parameters that can be passed to the application.

```
Usage:
  KasaLink [options] <device> <command>

Arguments:
  <device>   Kasa device name.
  <command>  Command to run.

Options:
  -v, --verbose   Show device messages.
  --version       Show version information
  -?, -h, --help  Show help and usage information
```

Based on the entries in the above example configuration file you can turn on the bulb using the following command.

`KasaLink.exe bulb bulbon --verbose`

The optional verbose argument emits both the request and response to the console for debugging purposes.

Finally the application return code is set upon completion and each successful operation should return zero.

`echo %errorlevel%`

## Credits

Reverse Engineering the TP-Link HS110
https://www.softscheck.com/en/reverse-engineering-tp-link-hs110/