import React from "react";
import {useMessage} from "../Hooks/Message";

export default () => {
    const message = useMessage("Hello World");
    return <div>{message}</div>;
};
