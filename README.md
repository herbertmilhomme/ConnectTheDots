# Intro

In the following documentation, all file paths are relative to the `technical-assessment/` directory contained in the [Technical Assessment Bundle](http://technical-assessment.konicaminoltamarketplace.com/technical-assessment.zip).

## Requirements

You will implement a game server (feel free to skip ahead to the rules). We have provided a web client to render the game and manage user interactions. Your server will communicate with the client through an API. The client is dumb and knows nothing about the game. It will be your responsibility to implement the game logic and maintain the game state. You can implement the server in any language you choose.

We have exposed three APIs to communicate with the Client (each API targets a different set of programming languages). You will find a full specification for each of the three APIs in the corresponding documentation. We have strived to ensure that the specification is clear, but if you are confused, feel free to request clarification. Regardless of which API you choose, your game server will be analyzed by a comprehensive test suite that does know the rules of the game - and all known corner cases. Your submission is not immediately discounted if it fails one or two of the tests in the suite; Your application will also be reviewed by several pairs of human eyes to assess code quality (microcosm) and application design (macrocosm).

You may provide your code to us by whatever means is most convenient. Along with your code, you should provide a README file with attribution for any external libraries or code as well as any specific instructions for running your application. You are welcome, but not required, to host your application.

## The Game

A connect-the-dots game for two players.

### Definitions

*   octilinear line - a horizontal, vertical, or 45° diagonal line

### Rules

*   The game is played on a 4x4 grid of 16 nodes.
*   Players take turns drawing octilinear lines connecting nodes.
*   Each line must begin at the start or end of the existing path, so that all lines form a continuous path.
*   The first line may begin on any node.
*   A line may connect any number of nodes.
*   Lines may not intersect.
*   No node can be visited twice.
*   The game ends when no valid lines can be drawn.
*   The player who draws the last line is the loser.

### Example Game

Each move is numbered. Lines that connect more that two nodes have each segment numbered. Player 1 made the odd numbered moves and Player 2 made the even numbered moves. Player 1 made the first move (1) and was forced to make the last move (9). Thus, Player 2 won.

![Example Game](https://technical-assessment.konicaminoltamarketplace.com/img/example-game.svg)

### Attribution

The game was designed by Sid Sackson.

## The Client

To reiterate, the Client does _not_ know the rules of the game. It is, effectively, a tool to plot lines on a grid. The grid is represented by quadrant IV of the Cartesian plane, with the caveat that the indices of the y-axis are positive. Example:

![Cartesian Plane](https://technical-assessment.konicaminoltamarketplace.com/img/cartesian-plane.svg)

The Client is configured for a 4x4 grid, but larger or smaller sizes are possible. Nodes on the grid are represented by a pair of Cartesian coordinates. Lines are represented by a starting node and an ending node. The Client has no conception of a continuous path; It blindly renders any line that it is given.

Additionally, the Client provides two text areas for displaying useful information to the user - a heading and a generic message field. The Client will never write any values to these text areas on its own; They are provided solely for your use.

## The APIs

Use the following guide to help choose the right API for you.

*   If you want to use JavaScript or a language that compiles to JavaScript: Use the Client API.
*   If you want to use another language (or Node.js), and your language supports the WebSocket protocol: Use the WebSocket API.
*   If your favorite language doesn’t have WebSocket support: Use the HTTP API.

The APIs provide nearly identical functionality and only differ meaningfully in network latency.

*   Client API : No network latency
*   WebSocket API : Mitigated network latency
*   HTTP API : Significant network latency (for a real-time game). Doesn’t support HTTP/2 streaming.

Your API choice will have no bearing whatsoever on our judgement of your solution.

## How to Get Started

Once you have decided which API you are going to use, you _must_ edit the second argument of the `Elm.Main.embed` function in the `client/init.js` file included in this bundle to set the correct API. If you are using the WebSocket or HTTP APIs, you _must_ provide the Client with the address of your server. There are three valid configuration options (listed below). The spelling and case of the `api` field _must_ be exactly as specified below. If your API requires the `hostname` field, you _must_ omit the trailing slash.

### Client API

```javascript
const app = Elm.Main.embed(node, {
    api: 'Client',
    hostname: '',
});
```

### HTTP API

```javascript
const app = Elm.Main.embed(node, {
    api: 'Http',
    hostname: 'http://localhost:8080',
});
```

### WebSocket API

```javascript
const app = Elm.Main.embed(node, {
    api: 'WebSocket',
    hostname: 'ws://localhost:8081',
});
```

If you are using the Client API, ensure that your code is loaded after `client/init.js`, and open `client/index.html` in your browser. If you are using the WebSocket or HTTP APIs, start your server, and open `client/index.html` in your browser.
