﻿using Rcl.Interop;
using Rosidl.Runtime;

namespace Rcl.Internal.Subscriptions;

internal unsafe class NativeSubscription<T> :
    NativeSubscriptionBase,
    IRclNativeSubscription where T : IMessage
{
    public NativeSubscription(
        RclNodeImpl node,
        string topicName,
        SubscriptionOptions options)
        : base(node, topicName, T.GetTypeSupportHandle(), options)
    {
    }

    protected override RosMessageBuffer TakeMessage()
    {
        // TODO: Parse this as RclFoxy.rmw_message_info_t
        // if need to access header fields on foxy.
        // Defined as RclHumble.rmw_message_info_t only because it has bigger size
        // to be compatible with both foxy and humble.
        //
        // Size of the GID was changed from 24 bytes to 16 bytes since iron.
        // We can still use RclHumble.rmw_message_info_t because it's bigger in size,
        // unless we need to access rmw_message_info_t.from_intra_process.
        RclHumble.rmw_message_info_t header;

        var rosMessage = RosMessageBuffer.Create<T>();
        if (rcl_ret_t.RCL_RET_OK ==
            rcl_take(Handle.Object, rosMessage.Data.ToPointer(), &header, null))
        {
            return rosMessage;
        }
        else
        {
            rosMessage.Dispose();
            return RosMessageBuffer.Empty;
        }
    }
}
