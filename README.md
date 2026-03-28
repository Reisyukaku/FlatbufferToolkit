# FlatbufferToolkit
Toolkit for rapid flatbuffer reverse engineering.

# Screenshot
![toolkit](https://i.imgur.com/Vy6O23v.png)

# MCP Server
Add the following to your mcp config json
```json
{
  "mcpServers": {
    "flatbuffer_toolkit": {
      "serverUrl": "http://localhost:8765/sse"
    }
  }
}
```

# Requirements
- .Net 10.0