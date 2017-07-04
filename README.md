
# KinectCamV2

# OBS
This fork focuses on making Kinect v2 usable with OBS. Issues with OBS has been solved, and CPU usage has been dropped to near nothing.
The functionality for mirror, zoom and face tracking has been disabled because it was using a lot of CPU (~8% on my computer).

Only tested on 32-bit OBS, but compile should work for both 32-bit and 64-bit.

# Other
Currently registers as a 32-bit webcamera. Should work with any webcam compatible app.

## Skype for Business
Also from http://codingbytodesign.net/2014/07/20/kinectcamv2-for-kinect-v2/:
Many people want to use this solution for Skype for Business 2016 or older. The solution is very simple, because Skype for Business 2016 and older uses .NET Framework 2.0 runtime and KinectCamV2 driver is written in .NET Framework 4.0, so to force Skype for Business 2016 or older to use .NET Framework 4.0 all you have to do is very simple thing. You have to create lync.exe.config file with following content.
```xml
<?xml version="1.0" encoding="utf-8"?>
<configuration>
    <startup> 
        <supportedRuntime version="v4.0" sku=".NETFramework,Version=v4.0"/>
    </startup>
</configuration>
```
And copy it to the place where you have lync.exe installed, for example inside folder: “C:\Program Files (x86)\Microsoft Office\Root\Office16”. This solution can be used for other solutions that uses .NET Framework and you need force to use higer version of .NET Framework. 

## Code history
Original code is from Piotr Sowa:
  * http://codingbytodesign.net/2014/07/20/kinectcamv2-for-kinect-v2/
  * http://codingbytodesign.net/2015/02/08/coding-by-to-design-of-kinectcamv2/
Modified by David Obando
  * https://github.com/DavidObando/KinectCamV2

Looks like original code may have been borowed from 
Maxim Kartavenkov post which pre-dates Piotrs code with ~2 years.
* https://www.codeproject.com/Articles/437617/DirectShow-Virtual-Video-Capture-Source-Filter-in
