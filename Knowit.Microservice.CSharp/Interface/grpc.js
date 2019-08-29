const child_process = require("child_process");
const glob = require("glob");

const files = glob.sync("../Contracts/*.proto");
child_process.execSync([
  "protoc",
  ...files,
  "--proto_path=..",
  "--js_out=import_style=commonjs:.",
  "--grpc-web_out=import_style=commonjs+dts,mode=grpcwebtext:."
].join(" "));
