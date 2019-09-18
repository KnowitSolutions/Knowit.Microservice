const child_process = require("child_process");
const glob = require("glob");
const fs = require("fs");

try {
  fs.mkdirSync("Contracts");
} catch (error) {
  if (error.code !== "EEXIST") throw error;
}

const files = glob.sync("../Contracts/*.proto");
child_process.execSync([
  "protoc",
  ...files,
  "--proto_path=../Contracts",
  "--js_out=import_style=commonjs:Contracts",
  "--grpc-web_out=import_style=commonjs+dts,mode=grpcwebtext:Contracts"
].join(" "));
