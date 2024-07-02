## Ripe Sdk

Welcome to the node.js [Ripe](https://app.ripecloud.io) Sdk! This package includes what you need to get started integrating your application with [Ripe](https://app.ripecloud.io) in a node.js project.

To get started run 
```
npm install @ripe-innovations-inc/ripe-sdk
```

And in your TS file import
```ts
import { RipeSdk, RipeOptions } from '@ripe-innovations-inc/ripe-sdk'
```

First, lets define some custom types to match what we're expecting the Ripe environment to return with:
```ts
type TestConfig = {
    my: TestConfigChild
}

type TestConfigChild = {
    string: string,
    bool: bool,
    json: any
}
```

Then you can then implement the Ripe Sdk:
```ts
function getConfig(){
    var options: RipeOptions = {
        uri: '<RIPE ENDPOINT>',
        key: '<RIPE KEY>',
        schema: [
            "my.string",
            "my.bool",
            "my.json"
        ]
    }
    var sdk = new RipeSdk<TestConfig>(options);
    var config = await sdk.hydrate();
    return config.data;
}
getConfig().then(console.log);

```
The code above will return your configuration with the specified keys:
```json
{
    "my":{
        "string": "",
        "bool": false,
        "json":{}
    }
}
```
You can store the `RipeSdk<T>` object for reusing later, and just call `sdk.hydrate()` on it to get the newest configuration.

