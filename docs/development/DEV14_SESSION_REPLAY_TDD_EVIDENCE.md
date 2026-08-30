# Dev.14 replay/disconnect/reconnect TDD evidence

## Prior frozen authority
Dev.13 exact main: `19482bc21460f84096e350f730065988239fbd3c`, tree `53f8638cb7be0bec1e0175569a8b22c009d3d771`, run `33319810581` all three jobs successful. Deterministic workspace SHA-256: `c14ac057f11fb9d47eac7687ec73e59b0aa1f3658cf9b361e83bc325b051743a`.

## RED
Commit `34b0401ed139efe55f76037b55d8e749e30afc0b`, run `33327339694`, managed `99299712127`.

The new replay acceptance test failed exactly at `Assert.True(replay.Accepted)` after first admission had been sent and the client disconnected before reading the response. Total: 250; succeeded: 249; failed: 1; skipped: 0. Release build, native Explorer, unsigned package, direct/registered COM Invoke and real product conversion smoke passed before the test failure.

## GREEN
Queue request lookup commit `7766cb1f7831356de08e2288ac2da51bcfee743d`.

Behavior commit `46da899ec7dad5ebe2acc934dbaf7c009abc0c26`, run `33327473492`, managed `99300068738`.

Observed: 18/18 locked restore; dependency audit PASS with 0 vulnerable-result packages; Release build PASS 0 warnings/0 errors; native Explorer/package/direct+registered COM/product conversion PASS; 250/250 managed tests; 78/78 static tests; 5/5 contract vectors. Real product smoke preserved source and pre-existing destination, published numbered output, and ffprobe verified MP3 at 320000 bit/s.

The deterministic A/B source workspace builds were byte-identical at SHA-256 `d6b3b56b3cf3f84c10a86c652e634f75f052f749b2cd31420169aba7f9dab73a`, 420896 bytes, 349 files. Embedded-manifest verification then failed at changed `src/Converty.Host/Ipc/HostRequestHandler.cs`, which is the expected pre-sync generated-authority boundary.
