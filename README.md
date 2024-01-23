Quick telem parser to try to identify dropped packets.
Works by detecting missing schema indexes.

Requires dot net.

Example:
```
tsharpe@dev-08:~/investigations/csmc$ dotnet run /home/tsharpe/investigations/csmc/data_trimmed_lowspeed.pcap
3719 - Missing idx for schema: 16CEFD6A1ABE5F42459DA6D4B4CC9195, expected 131 at 17AB907CE60FBC00
    Last seen: 130, Current:132
3720 - Missing idx for schema: C087012F261F7F430A2A55F0FC4726BF, expected 131 at 17AB907CE60FBC00
    Last seen: 130, Current:132
3721 - Missing idx for schema: 58307EA58C9A2F968B372EEEAB84EA7B, expected 131 at 17AB907CE60FBC00
    Last seen: 130, Current:132
```