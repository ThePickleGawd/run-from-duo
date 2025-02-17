using System;
using Newtonsoft.Json;

// Class for parsing the function call JSON
[Serializable]
public class FunctionCallResponse
{
    public string type;
    public string name;
    public string arguments;
}

[Serializable]
public class FunctionArguments
{
    public string reward;
}