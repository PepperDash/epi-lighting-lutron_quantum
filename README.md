# PDT.EssentialsPluginTemplate.EPI

Example Config: 


``` json
{
    "system": 
    {
    },
    "system_url": "http://portal-QA.devcloud.pepperdash.com/templates/0f50640b-bc89-42d5-998f-81d137d3fc98#/template_summary",
    "template": 
    {
        "devices": [
        {
                "key": "lighting-1",
                "uid": 1920,
                "name": "Lutron  QS Scenes 1",
                "type": "lutronQuantum",
                "group": "lighting",
                "properties": 
                {
                    "username": "admin",
                    "password": "lutron",
                    "IntegrationId": "375",
                    "Scenes": [
                        {
                            "name": "All On",
                            "id": 5
                        },
                        {
                            "name": "Utility",
                            "id": 1
                        },
                        {
                            "name": "General Lecture",
                            "id": 2
                        },
                        {
                            "name": "Projection",
                            "id": 3
                        },
                        {
                            "name": "Lab Only",
                            "id": 4
                        },
                        {
                            "name": "All Off",
                            "id": 0
                        }
                    ],
                    "control": 
                    {
                        "controlPortDevKey": "processor",
                        "comParams": 
                        {
                            "dataBits": 8,
                            "softwareHandshake": "None",
                            "baudRate": 115200,
                            "parity": "None",
                            "stopBits": 1,
                            "hardwareHandshake": "None",
                            "protocol": "RS232"
                        },
                        "method": "com",
                        "controlPortNumber": 6
                    }
                }
            },
       ]
   }    
}       
```
