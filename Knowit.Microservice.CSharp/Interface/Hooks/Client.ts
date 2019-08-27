import {CorePromiseClient} from "../Contracts/ProjectName_grpc_web_pb";

const client = new CorePromiseClient("", null, null);
export const useClient = () => client;
