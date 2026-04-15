// VMS MTH$RANDOM, old glibc
// https://en.m.wikipedia.org/wiki/Linear_congruential_generator
uint32_t VMS_Random(uint32_t state) {
    const uint32_t multiplier = 69069;
    const uint32_t increment = 1;
    return (state * multiplier) + increment;
}

void GenMasterSeed() {
    uint32_t keySeed[0x180];
    
    // this copies 4 ints at a time from a randomized byte stream.
    // i manually outlined it because it's 96 times keySeed[n] = globalKeySeed[n];
    GenKeySeed(keySeed);

    uint32_t lcgState = 0x7Bu;
    for (auto i = 0; i < sizeof(keySeed); ++i) {
        lcgState = VMS_Random(lcgState);
        keySeed[i] ^= (lcgState >> 16);
    }

    char* masterKey = g_MasterKey;
    lcgState = 0x7B;

    for (auto i = 0; i < sizeof(keySeed), ++i) {
        lcgState = VMS_Random(lcgState);
        uint8_t seedByte = (uint8_t)(keySeed[i] & 0xFF);
        masterKey[i] = seedByte - (uint8_t)(val >> 16);
    }

    masterKey[sizeof(keySeed)] = 0; 
}
