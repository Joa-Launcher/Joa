import {HubConnection, HubConnectionBuilder, RetryContext} from "@microsoft/signalr";
import {useEffect, useState} from "react";
import PluginCommand from "../models/pluginCommand";
import {executeCommandMethod, updateCommandsMethod} from "../models/JoaMethods";

export function useJoaSearch() : [HubConnection | undefined] {
    const [connection, setConnection] = useState<HubConnection>();

    useEffect(() => {
        const newConnection = new HubConnectionBuilder()
            .withUrl("http://localhost:5000/searchHub")
            .withAutomaticReconnect({
                nextRetryDelayInMilliseconds(retryContext: RetryContext): number | null {
                    return 1000;
                }})
            .build();

        newConnection.onreconnected(() => {
            console.log("reconnectedd");
            setConnection(structuredClone(connection));
        })

        newConnection.start().then(() => {
            console.log("start");
            setConnection(newConnection);
        });

        return () => {
            console.log("stop");
            newConnection.stop().then();
        }
    }, []);

    return [connection]
}

export function useCommands(connection: HubConnection) : [PluginCommand[], (searchString: string) => void, () => void]{
    const [ searchResults, setSearchResults ] = useState<PluginCommand[]>([]);
    const updateCommands = async (searchString: string) => {
        const commands = await connection.invoke<PluginCommand[]>(updateCommandsMethod, searchString);
        console.log(commands.length);
        setSearchResults(commands.slice(0,8));
    }

    const clearCommands = () => {
        setSearchResults([]);
    }

    return [searchResults, updateCommands, clearCommands];
}

export function useSelectedCommand(commands: PluginCommand[]) : [ number, () => void, () => void, () => void]{
    const [ activeIndex, setActiveIndex ] = useState(0);

    const moveDown = () => {
        if(activeIndex < commands.length)
            setActiveIndex(activeIndex + 1);
    }

    const moveUp = () => {
        if(activeIndex > 0)
            setActiveIndex(activeIndex - 1);
    }

    const reset = () => {
        setActiveIndex(0);
    }

    return [ activeIndex, moveUp, moveDown, reset ]
}

export function executeCommand(connection: HubConnection, command: PluginCommand) {
    connection.invoke(executeCommandMethod, command.commandId)
        .catch(function (err : any) {
            return console.error(err.toString());
        });
}
