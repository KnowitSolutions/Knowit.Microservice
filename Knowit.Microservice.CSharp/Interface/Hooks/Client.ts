import {CorePromiseClient} from "../Contracts/projectname/api/services_grpc_web_pb";

const client = new CorePromiseClient("", null, null);
export const useClient = () => client;
