import {useEffect, useState} from "react";
import {useClient} from "./Client";
import {EchoRequest} from "../Contracts/projectname/api/echo_pb";

export const useMessage = (requestMessage) => {
    const client = useClient();
    const [responseMessage, setResponseMessage] = useState();

    useEffect(() => {
        let ignored = false;

        (async () => {
            const request = new EchoRequest();
            request.setMessage(requestMessage);

            let message;
            try {
                const response = await client.echo(request);
                message = response.getMessage();
            }
            catch (error) {
                message = error.message;
            }
            
            if (ignored) return;
            setResponseMessage(message);
        })();

        return () => ignored = true;
    }, []);

    return responseMessage;
};