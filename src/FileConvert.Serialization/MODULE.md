# FileConvert.Serialization

Strict versioned JSON adapter boundary for engine-independent FileConvert contracts.

Allowed responsibilities:
- exact JSON property naming and v1 schema dispatch;
- stable enum-to-wire-string mapping;
- unknown/duplicate member rejection;
- mapping between JSON wire models and validated immutable Contracts.

Forbidden responsibilities:
- named pipes or any transport;
- filesystem mutation or path canonicalization;
- media probing/parsing;
- process creation, engine arguments, native loading, FFmpeg/WIC calls;
- networking;
- provider/plugin discovery or execution.

Dependency direction is `FileConvert.Serialization -> FileConvert.Contracts` only.
