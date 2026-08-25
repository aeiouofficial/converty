# B2 connected-server identity gate

B2 is not fully closed after dev.8. Current-user pipe ACLs, Host-side client SID validation, fixed installed Host startup, and bounded startup retries are qualified, but Bridge still requires executable acceptance evidence that each connected server session belongs to the expected installed Converty Host identity before application data is trusted.

The final rule must match the actual package/signing model and fail closed when identity cannot be established. Fixed path, pipe name, or same-user ownership alone must not be described as complete server authentication. Production signing private keys must remain outside the repository/workspace.

Until that evidence exists, B3 remains blocked and Converty must not claim complete pipe-squatting resistance or signed server authentication.
