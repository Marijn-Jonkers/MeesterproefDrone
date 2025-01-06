const WebSocket = require('ws');
const readline = require('readline');

// Set up WebSocket Server
const wss = new WebSocket.Server({ port: 8080 });

wss.on('connection', (ws) => {
    console.log('Client connected to WebSocket server');

    // Listen for commands from the console
    const rl = readline.createInterface({
        input: process.stdin,
        output: process.stdout,
    });

    rl.on('line', (line) => {
        // Send commands to Unity
        ws.send(line.trim());
    });

    ws.on('message', (message) => {
        console.log(`Message from Unity: ${message}`);
    });

    ws.on('close', () => {
        console.log('Client disconnected');
        rl.close();
    });
});

console.log('WebSocket server running on ws://localhost:8080');
