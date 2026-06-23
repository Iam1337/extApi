# extApi - Simple Http Server for Unity

Created by [iam1337](https://github.com/iam1337)

![](https://img.shields.io/badge/unity-2022.1%20or%20later-green.svg)
[![⚙ Build and Release](https://github.com/Iam1337/extApi/actions/workflows/ci.yml/badge.svg)](https://github.com/Iam1337/extApi/actions/workflows/ci.yml)
[![openupm](https://img.shields.io/npm/v/com.iam1337.extapi?label=openupm&registry_uri=https://package.openupm.com)](https://openupm.com/packages/com.iam1337.extapi/)
[![](https://img.shields.io/github/license/iam1337/extApi.svg)](https://github.com/Iam1337/extApi/blob/master/LICENSE)
[![semantic-release: angular](https://img.shields.io/badge/semantic--release-angular-e10079?logo=semantic-release)](https://github.com/semantic-release/semantic-release)

### Table of Contents
- [Introduction](#introduction)
- [Installation](#installation)
- [Examples](#examples)
- [Author Contacts](#author-contacts)

## Introduction
extApi - It is a simple Http Server, which requires a minimum of specific code to work with. 

### Features:

## Installation
**Old school**

Just copy the [Assets/extApi](Assets/extApi) folder into your Assets directory within your Unity project.

**OpenUPM**

Via [openupm-cli](https://github.com/openupm/openupm-cli):<br>
```
openupm add com.iam1337.extapi
```

Or if you don't have it, add the scoped registry to manifest.json with the desired dependency semantic version:
```
"scopedRegistries": [
	{
		"name": "package.openupm.com",
		"url": "https://package.openupm.com",
		"scopes": [
			"com.iam1337.extapi",
		]
	}
],
"dependencies": {
	"com.iam1337.extapi": "1.0.0"
}
```

## Examples

To make a simple Web Api, the following lines are enough:
```csharp
// Create Api Server
_api = new Api();
_api.AddController(new ApiController()); // <--- Add controller
_api.Listen(8080, IPAddress.Any, IPAddress.Loopback);

// Simple controller example
[ApiRoute("api")]
public class ApiController
{
        [ApiGet("vector/{x}/{y}/{z}")] // GET /api/vector/1/2.5/5
        public ApiResult GetVector(float x, float y, float z)
        {
		return ApiResult.Ok(new Vector3(x, y, z));
        }


        [ApiPost("vector")] // POST /api/vector { "x": 1.0, "y": 2.5, "z": 5.0 }
        public ApiResult GetVector([ApiBody] Vector vector)
        {
		// TODO: ...
        }
}
```

## Author Contacts
\> [telegram.me/iam1337](http://telegram.me/iam1337) <br>
\> [ext@iron-wall.org](mailto:ext@iron-wall.org)

## License
This project is under the MIT License.

