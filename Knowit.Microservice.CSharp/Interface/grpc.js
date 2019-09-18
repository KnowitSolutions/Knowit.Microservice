const child_process = require("child_process");
const glob = require("glob");
const fs = require("fs");

fs.mkdirSync("Contracts");

const files = glob.sync("../Contracts/*.proto");
child_process.execSync([
  "protoc",
  ...files,
  "--proto_path=../Contracts",
  "--js_out=import_style=commonjs:Contracts",
  "--grpc-web_out=import_style=commonjs+dts,mode=grpcwebtext:Contracts"
].join(" "));
